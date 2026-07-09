// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Reflection;
using Facebook.SocialVR.Core.Inputs;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.Canvas.SetupSequence;
using Facebook.Workrooms.InputDevice;
using Facebook.Workrooms.Navigation;
using Facebook.Workrooms.Surfaces;
using Facebook.Workrooms.Testing;
using Facebook.Workrooms.TouchInputInteractor;
using Facebook.Workrooms.Whiteboard;
using Facebook.Workrooms.WhiteboardTool;
using Facebook.Xplat.Events;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assert = UnityEngine.Assertions.Assert;

namespace Facebook.Workrooms.UnitTests {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class WhiteboardToolControllerTest {
    private DispatcherSpyDecorator dispatcher_;
    private GameObject testRoot_;
    private IServiceContainer container_;
    private WhiteboardToolState state_;
    private Collider whiteboardCollider_;
    private FakeTouchInputInteractor leftTouchInputInteractor_;
    private ITouchInputInteractorTarget whiteboardTouchInputInteractorTarget_;
    private WhiteboardToolAttachArgs defaultAttachState_;
    private FakeWhiteboardToolTips whiteboardToolTips_;
    private IWhiteboardController whiteboardController_;
    private ICanvasInstance canvasInstance_;
    private IWhiteboardToolsStateService whiteboardToolsStateService_;
    private INavigationService navigationService_;

    private const string VIBRATION_FIELD = "vibration_";

    [OnCall(OnCallName.workrooms_creative_collaborations)]
    class FakeWhiteboardToolTips : IWhiteboardToolTips {
      public IWhiteboardToolTip FakeTip;
      public IWhiteboardToolTip ActiveTip { get; private set; }

      public void SetActiveTip(IWhiteboardToolTip tip) {
        ActiveTip = tip;
      }

      public IWhiteboardToolTip GetTip(InputMethod inputMethod, Handedness hand) {
        return inputMethod != InputMethod.NONE ? FakeTip : null;
      }

      public void Init(IInputMethodService inputMethodService) { }
    }

    [Emits(typeof(TouchInputInteractorChangeTargetEvent))]
    [OnCall(OnCallName.workrooms_creative_collaborations)]
    class FakeTouchInputInteractor : ITouchInputInteractor {
      private bool whiteboardCollisionEnabled_ = true;
      private ITouchInputInteractorTarget whiteboard_;
      public Handedness Handedness { get; }
      public Vector3? CurrentInputPosition { get; }
      public Collider CurrentHitCollider { get; }
      public bool IsTouchingWhiteboard { get; set; }

      public FakeTouchInputInteractor(ITouchInputInteractorTarget whiteboard) {
        whiteboard_ = whiteboard;
      }

      public void Dispose() { }

      public ITouchInputInteractorTarget CurrentHitInputTarget {
        get { return IsTouchingWhiteboard && whiteboardCollisionEnabled_ ? whiteboard_ : null; }
      }

      public Vector3 GetCollisionPosition(Transform hitSurface) {
        return Vector3.zero;
      }

      public void DisableInteractionForType(TouchInputInteractorTarget.Type targetType) {
        if (targetType == TouchInputInteractorTarget.Type.WHITEBOARD) {
          whiteboardCollisionEnabled_ = false;
        }
      }

      public void EnableInteractionForType(TouchInputInteractorTarget.Type targetType) {
        if (targetType == TouchInputInteractorTarget.Type.WHITEBOARD) {
          whiteboardCollisionEnabled_ = true;
        }
      }

    }

    [SetUp]
    public void Setup() {
      container_ = ServiceLocator.CreateServiceContainer(SceneManager.GetActiveScene().path);
      dispatcher_ = new DispatcherSpyDecorator(new Dispatcher(), Substitute.For<IDispatcher>());
      container_.Bind<IDispatcher>().To(dispatcher_);

      var logger = Substitute.For<ILogService>();
      container_.Bind<ILogService>().To(logger);

      // init monob
      testRoot_ = new GameObject();
      testRoot_.SetActive(false);
      whiteboardCollider_ = testRoot_.AddComponent<BoxCollider>();
      whiteboardTouchInputInteractorTarget_ = Substitute.For<ITouchInputInteractorTarget>();
      whiteboardTouchInputInteractorTarget_.TargetType.Returns(TouchInputInteractorTarget.Type.WHITEBOARD);

      // init attach states
      defaultAttachState_ = new WhiteboardToolAttachArgs(
        Handedness.LEFT,
        true,
        InputMethod.FLIPPED_CONTROLLER,
        testRoot_.transform
      );

      leftTouchInputInteractor_ = new FakeTouchInputInteractor(whiteboardTouchInputInteractorTarget_);
      var fakeTip = Substitute.For<IWhiteboardToolTip>();
      fakeTip.TouchInputInteractor.Returns(leftTouchInputInteractor_);
      whiteboardToolTips_ = new FakeWhiteboardToolTips() { FakeTip = fakeTip };
    }

    private void SetPrivateField(object controller, string fieldName, object fieldValue) {
      var property = controller.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
      property?.SetValue(controller, fieldValue);
    }

    [TearDown]
    public void Cleanup() {
      Object.DestroyImmediate(testRoot_);
      ServiceLocator.Clear();
      container_.Clear();
      ServiceLocator.RemoveServiceContainer(SceneManager.GetActiveScene().path);
    }

    [Test]
    public void WhiteboardDrawingStateMatchesToolsStateDrawingState() {
      var controller = CreateController();

      // Doesn't trigger event with default state
      controller.UpdateState();
      Assert.AreEqual(DrawingState.IDLE, state_.DrawingState);

      // Drawing state changes to active when ToolsState indicates that the left tool is drawing
      whiteboardToolsStateService_.ToolsState.Returns(new ToolsState {IsLeftToolDrawing = true});
      controller.UpdateState();
      Assert.AreEqual(DrawingState.ACTIVE, state_.DrawingState);

      // Returns to idle when left tool in ToolsState stops drawing
      whiteboardToolsStateService_.ToolsState.Returns(new ToolsState {IsLeftToolDrawing = false});
      controller.UpdateState();
      Assert.AreEqual(DrawingState.IDLE, state_.DrawingState);
    }

    [Test]
    public void WhiteboardDrawingResumesWhenToolIsReattached() {
      var controller = CreateController();

      // Drawing state changes to active when ToolsState indicates that the left tool is drawing
      whiteboardToolsStateService_.ToolsState.Returns(new ToolsState {IsLeftToolDrawing = true});
      controller.UpdateState();
      Assert.AreEqual(DrawingState.ACTIVE, state_.DrawingState);

      // Switch to inactive attach state (eg, whiteboard tool not attached after inactivity)
      var attachStateInputTypeNone = state_.AttachState;
      attachStateInputTypeNone.IsActive = false;
      state_.AttachState = attachStateInputTypeNone;
      controller.UpdateState();
      Assert.AreEqual(DrawingState.IDLE, state_.DrawingState);

      // Switch back to active attach state, drawing should resume
      attachStateInputTypeNone.IsActive = true;
      state_.AttachState = attachStateInputTypeNone;
      controller.UpdateState();
      Assert.AreEqual(DrawingState.ACTIVE, state_.DrawingState);
    }

    [Test]
    public void WhenToolAttachStateInactiveThenActiveTipIsNull() {
      var controller = CreateController();

      controller.UpdateState();
      Assert.IsNotNull(state_.ActiveTip);

      // Switch to inactive attach state (eg, whiteboard tool not attached because it's outside interaction bounds)
      var attachStateInputTypeNone = state_.AttachState;
      attachStateInputTypeNone.IsActive = false;
      state_.AttachState = attachStateInputTypeNone;
      controller.UpdateState();

      // Ensure active tip is null, ie, WorkroomsInputCollider is disabled, ceasing any drawing or image moving
      Assert.IsNull(state_.ActiveTip);
    }

    [Test]
    public void ActiveCollisionTargetTriggersVibration() {
      var controller = CreateController();

      var startCount = 0;
      var stopCount = 0;
      var vibration = Substitute.For<IWhiteboardToolVibration>();
      vibration.When(x => x.StartVibration()).Do(x => startCount++);
      vibration.When(x => x.StopVibration()).Do(x => stopCount++);
      SetPrivateField(controller, VIBRATION_FIELD, vibration);

      leftTouchInputInteractor_.IsTouchingWhiteboard = true;
      controller.UpdateState();
      Assert.AreEqual(1, startCount);
      Assert.AreEqual(0, stopCount);

      // Doesn't start multiple times
      vibration.VibrationActive.Returns(true);
      controller.UpdateState();
      Assert.AreEqual(1, startCount);
      Assert.AreEqual(0, stopCount);

      leftTouchInputInteractor_.IsTouchingWhiteboard = false;
      controller.UpdateState();
      Assert.AreEqual(1, startCount);
      Assert.AreEqual(1, stopCount);

      // cleanup
      state_.AttachState = defaultAttachState_;
    }

    [Test]
    public void AttachStateInactiveStopsVibration() {
      var controller = CreateController();

      var stopCount = 0;
      var vibration = Substitute.For<IWhiteboardToolVibration>();
      vibration.When(x => x.StopVibration()).Do(x => stopCount++);
      SetPrivateField(controller, VIBRATION_FIELD, vibration);

      // Start vibration
      leftTouchInputInteractor_.IsTouchingWhiteboard = true;
      controller.UpdateState();
      vibration.VibrationActive.Returns(true);

      // Set attach state inactive
      var invalidInputMethodAttachState = state_.AttachState;
      invalidInputMethodAttachState.IsActive = false;
      state_.AttachState = invalidInputMethodAttachState;
      controller.UpdateState();

      Assert.AreEqual(1, stopCount);
    }

    [Test]
    public void WhenDrawingBlockedByPopupsVibrationIsStopped() {
      var controller = CreateController();

      var stopCount = 0;
      var vibration = Substitute.For<IWhiteboardToolVibration>();
      vibration.When(x => x.StopVibration()).Do(x => stopCount++);
      SetPrivateField(controller, VIBRATION_FIELD, vibration);

      // Start vibration
      leftTouchInputInteractor_.IsTouchingWhiteboard = true;
      controller.UpdateState();
      vibration.VibrationActive.Returns(true);

      canvasInstance_.PopupController.AllowWhiteboardInput().Returns(false);
      controller.UpdateState();

      Assert.AreEqual(1, stopCount);
    }

    [Test]
    public void WhiteboardDrawingEventsNotDispatchedForDefaultState() {
      var controller = CreateController();

      // Doesn't trigger event with default state
      controller.UpdateState();
      dispatcher_.Spy.DidNotReceive().Dispatch(Arg.Is<WhiteboardDrawingStartedEvent>(e => true));
      dispatcher_.Spy.DidNotReceive().Dispatch(Arg.Is<WhiteboardDrawingStoppedEvent>(e => true));
    }

    [Test]
    public void WhiteboardDrawingStateUnaffectedByOtherHandChanges() {
      var controller = CreateController();

      // Right controller starts drawing, left controller drawing state remains unchanged
      whiteboardToolsStateService_.ToolsState.Returns(new ToolsState {IsRightToolDrawing = true});
      controller.UpdateState();
      Assert.AreEqual(DrawingState.IDLE, state_.DrawingState);

      // Right controller stops drawing, left controller drawing state still remains unchanged
      whiteboardToolsStateService_.ToolsState.Returns(new ToolsState {IsRightToolDrawing = false});
      controller.UpdateState();
      Assert.AreEqual(DrawingState.IDLE, state_.DrawingState);

      // Left controller starts drawing, now drawing state becomes active
      whiteboardToolsStateService_.ToolsState.Returns(new ToolsState {IsLeftToolDrawing = true});
      controller.UpdateState();
      Assert.AreEqual(DrawingState.ACTIVE, state_.DrawingState);
    }

    [Test]
    public void TryRaycastPlaneReturnsSameValueForAllOrientations() {
      var plane = (new GameObject()).transform;
      plane.position = Vector3.zero;
      plane.rotation = Quaternion.Euler(0, 0, 0);
      var rayOffset = Vector3.forward * 2.0f;
      var direction = Vector3.forward;
      var p0 = CanvasHelpers.TryRaycastPlane(plane, rayOffset, -direction);
      var p1 = CanvasHelpers.TryRaycastPlane(plane, -rayOffset, -direction);
      var p2 = CanvasHelpers.TryRaycastPlane(plane, rayOffset, direction);
      var p3 = CanvasHelpers.TryRaycastPlane(plane, -rayOffset, direction);
      plane.rotation = Quaternion.Euler(0, 180, 0);
      var p4 = CanvasHelpers.TryRaycastPlane(plane, rayOffset, -direction);
      var p5 = CanvasHelpers.TryRaycastPlane(plane, -rayOffset, -direction);
      var p6 = CanvasHelpers.TryRaycastPlane(plane, rayOffset, direction);
      var p7 = CanvasHelpers.TryRaycastPlane(plane, -rayOffset, direction);
      Assert.IsTrue(p0 == p1);
      Assert.IsTrue(p1 == p2);
      Assert.IsTrue(p2 == p3);
      Assert.IsTrue(p3 == p4);
      Assert.IsTrue(p4 == p5);
      Assert.IsTrue(p5 == p6);
      Assert.IsTrue(p6 == p7);
    }

    private WhiteboardToolController CreateController() {
      var canvasSetupService = Substitute.For<ICanvasSetupService>();
      var deskCanvasSetupService = Substitute.For<ICanvasSetupController>();
      var wallCanvasSetupService = new DeskCanvasSetupController.DummyCanvasSetupController();
      canvasSetupService.GetSetupController(WorkroomsLocation.DESK).Returns(deskCanvasSetupService);
      canvasSetupService.GetSetupController(WorkroomsLocation.WALL).Returns(wallCanvasSetupService);
      deskCanvasSetupService.IsCalibrationSetupStep.Returns(false);
      deskCanvasSetupService.IsSetupActive.Returns(false);
      whiteboardController_ = Substitute.For<IWhiteboardController>();
      whiteboardController_.Collider.Returns(whiteboardCollider_);
      canvasInstance_ = Substitute.For<ICanvasInstance>();
      canvasInstance_.PopupController.AllowWhiteboardInput().Returns(true);
      canvasInstance_.Whiteboard.Returns(whiteboardController_);
      var instanceContainer = Substitute.For<ICanvasInstanceContainer>();
      canvasInstance_.Container.Returns(instanceContainer);
      state_ = new WhiteboardToolState();
      state_.AttachState = defaultAttachState_;
      state_.GetClosestCanvasFunc = (p) => canvasInstance_;
      whiteboardToolsStateService_ = Substitute.For<IWhiteboardToolsStateService>();
      navigationService_ = Substitute.For<INavigationService>();
      leftTouchInputInteractor_.IsTouchingWhiteboard = false;
      var touchEnabledController = Substitute.For<ITouchEnabledController>();
      touchEnabledController.TouchInputEnabledForHand(Handedness.LEFT).ReturnsForAnyArgs(true);
      return new WhiteboardToolController(
        state_,
        whiteboardToolTips_,
        Substitute.For<IWhiteboardToolVibration>(),
        dispatcher_,
        whiteboardToolsStateService_,
        navigationService_,
        canvasSetupService,
        touchEnabledController
      );
    }
  }
}
