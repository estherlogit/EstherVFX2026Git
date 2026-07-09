// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using Facebook.SocialVR.Core.Bedrock;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.Canvas.Definitions;
using Facebook.Workrooms.Canvas.SetupSequence;
using Facebook.Workrooms.InputDevice;
using Facebook.Workrooms.LaserPointer;
using Facebook.Workrooms.Navigation;
using Facebook.Workrooms.ScreenSharing;
using Facebook.Workrooms.Surfaces;
using Facebook.Workrooms.TouchInputInteractor;
using Facebook.Workrooms.WhiteboardTool;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Facebook.Workrooms.Surfaces.WorkroomsLocation;

namespace Facebook.Workrooms.Tests.UnitTests.Whiteboard {

  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class CanvasAllowedInputCheckerTest {
    private IServiceContainer container_;
    private CanvasAllowedInputChecker deskAllowedInputFilter_;
    private CanvasAllowedInputChecker wallAllowedInputFilter_;
    private ICanvasInputController canvasInputController_;
    private IInputInteractor whiteboardToolLaserInteractor_;
    private IInputInteractor defaultLaserInteractor_;
    private IInputInteractor whiteboardToolTouchInteractor_;
    private IWhiteboardToolsService whiteboardToolsService_;
    private IWhiteboardToolsStateService whiteboardToolsStateService_;
    private IDevicePlatform devicePlatform_;
    private ILocalPlayerSurfaceAnchorController localPlayerSurfaceAnchorController_;
    private ICanvasInstance canvasInstance_;
    private INavigationService navigationService_;
    private ICanvasDriver canvasDriver_;
    private ICanvasSetupController canvasSetupController_;
    private IMainScreenController mainScreenController_;

    [SetUp]
    public void Setup() {
      container_ = ServiceLocator.CreateServiceContainer(SceneManager.GetActiveScene().path);

      var logger = Substitute.For<ILogService>();
      container_.Bind<ILogService>().To(logger);

      canvasInputController_ = Substitute.For<ICanvasInputController>();

      whiteboardToolLaserInteractor_ = Substitute.For<IWorkroomsLaserPointer>();
      whiteboardToolLaserInteractor_.CurrentInputPosition.Returns(Vector3.zero);

      defaultLaserInteractor_ = Substitute.For<IWorkroomsLaserPointer>();
      defaultLaserInteractor_.CurrentInputPosition.Returns(Vector3.zero);

      whiteboardToolTouchInteractor_ = Substitute.For<ITouchInputInteractor>();
      whiteboardToolTouchInteractor_.CurrentInputPosition.Returns(Vector3.zero);

      whiteboardToolsService_ = Substitute.For<IWhiteboardToolsService>();
      var whiteboardTool = Substitute.For<IWhiteboardTool>();
      whiteboardTool.HoverLaserPointer.Returns(whiteboardToolLaserInteractor_);
      whiteboardToolsService_.LeftTool.Returns(whiteboardTool);
      whiteboardToolsStateService_ = Substitute.For<IWhiteboardToolsStateService>();

      devicePlatform_ = Substitute.For<IDevicePlatform>();
      devicePlatform_.IsVREnabled().Returns(true);

      localPlayerSurfaceAnchorController_ = Substitute.For<ILocalPlayerSurfaceAnchorController>();

      var inputMethodService = Substitute.For<IInputMethodService>();
      inputMethodService.IsUsingHands().Returns(false);

      var canvasConfig = Substitute.For<CanvasConfig>();
      canvasConfig.DeskPUIMode.Returns(false);

      canvasInstance_ = Substitute.For<ICanvasInstance>();
      canvasInstance_.Whiteboard.IsHandTrackedDragEnabledAtWall.Returns(false);
      canvasInstance_.PopupController.Returns((IWhiteboardPopupController)null);

      navigationService_ = Substitute.For<INavigationService>();
      navigationService_.IsNavigating.Returns(false);
      
      canvasDriver_ = Substitute.For<ICanvasDriver>();
      canvasDriver_.LoadingState.IsLoading.Returns(false);
      
      canvasSetupController_ = Substitute.For<ICanvasSetupController>();
      canvasSetupController_.ShouldHideWhiteboard.Returns(false);
      
      mainScreenController_ = Substitute.For<IMainScreenController>();
      mainScreenController_.ViewType.Returns(MainScreenViewType.WHITEBOARD);

      deskAllowedInputFilter_ = new CanvasAllowedInputChecker(
        canvasInputController: canvasInputController_,
        devicePlatform: devicePlatform_,
        whiteboardToolsService: whiteboardToolsService_,
        whiteboardToolsStateService: whiteboardToolsStateService_,
        location: DESK,
        localPlayerSurfaceAnchorController: localPlayerSurfaceAnchorController_,
        inputMethodService: inputMethodService,
        isHandDrawingEnabled: false,
        canvasConfig: canvasConfig,
        canvasInstance: canvasInstance_,
        navigationService: navigationService_,
        canvasDriver: canvasDriver_,
        canvasSetupController: canvasSetupController_,
        mainScreenController: mainScreenController_
      );

      wallAllowedInputFilter_ = new CanvasAllowedInputChecker(
        canvasInputController: canvasInputController_,
        devicePlatform: devicePlatform_,
        whiteboardToolsService: whiteboardToolsService_,
        whiteboardToolsStateService: whiteboardToolsStateService_,
        location: WALL,
        localPlayerSurfaceAnchorController: localPlayerSurfaceAnchorController_,
        inputMethodService: inputMethodService,
        isHandDrawingEnabled: false,
        canvasConfig: canvasConfig,
        canvasInstance: canvasInstance_,
        navigationService: navigationService_,
        canvasDriver: canvasDriver_,
        canvasSetupController: canvasSetupController_,
        mainScreenController: mainScreenController_
      );
    }

    [TearDown]
    public void TearDown() {
      ServiceLocator.Clear();
      container_.Clear();
      ServiceLocator.RemoveServiceContainer(SceneManager.GetActiveScene().path);
    }

    [Test]
    public void DefaultLaserCanMoveElements() {
      AssertInput(DESK, InputType.CANVAS_ELEMENTS, ToolType.DEFAULT_LASER, assertInputAllowed: true);
      AssertInput(WALL, InputType.CANVAS_ELEMENTS, ToolType.DEFAULT_LASER, assertInputAllowed: true);
    }

    [Test]
    public void DefaultLaserCanDrawInDesktopMode() {
      devicePlatform_.IsVREnabled().Returns(true);
      AssertInput(DESK, InputType.WHITEBOARD, ToolType.DEFAULT_LASER, assertInputAllowed: false);
      devicePlatform_.IsVREnabled().Returns(false);
      AssertInput(DESK, InputType.WHITEBOARD, ToolType.DEFAULT_LASER, assertInputAllowed: true);
    }
    [Test]
    public void DefaultLaserCannotMoveElementWhenPenSelectedInDesktopMode() {
      // Desktop mode + pen mode = no element moving
      devicePlatform_.IsVREnabled().Returns(false);
      MockToolType(WhiteboardToolStateTypes.DRAWING);
      AssertInput(WALL, InputType.CANVAS_ELEMENTS, ToolType.DEFAULT_LASER, assertInputAllowed: false);
      // VR mode + pen mode = elements can be moved by forward laser interactor
      devicePlatform_.IsVREnabled().Returns(true);
      MockToolType(WhiteboardToolStateTypes.DRAWING);
      AssertInput(WALL, InputType.CANVAS_ELEMENTS, ToolType.DEFAULT_LASER, assertInputAllowed: true);
      // Desktop mode + select mode = elements can be moved by forward laser interactor
      devicePlatform_.IsVREnabled().Returns(false);
      MockToolType(WhiteboardToolStateTypes.SELECT);
      AssertInput(WALL, InputType.CANVAS_ELEMENTS, ToolType.DEFAULT_LASER, assertInputAllowed: true);
    }

    [Test]
    public void WhiteboardToolLaserCanPan() {
      MockHitElement(false);
      AssertInput(DESK, InputType.CANVAS_ELEMENTS, ToolType.WHITEBOARD_LASER, assertInputAllowed:true);
      AssertInput(WALL, InputType.CANVAS_ELEMENTS, ToolType.WHITEBOARD_LASER, assertInputAllowed:true);
    }

    [Test]
    public void WhiteboardToolLaserCanMoveElement() {
      MockHitElement(true);
      AssertInput(DESK, InputType.CANVAS_ELEMENTS, ToolType.WHITEBOARD_LASER, assertInputAllowed: true);
      AssertInput(WALL, InputType.CANVAS_ELEMENTS, ToolType.WHITEBOARD_LASER, assertInputAllowed: true);
    }

    [Test]
    public void WhiteboardToolLaserCanDraw() {
      AssertInput(DESK, InputType.WHITEBOARD, ToolType.WHITEBOARD_LASER, assertInputAllowed: true);
      AssertInput(WALL, InputType.WHITEBOARD, ToolType.WHITEBOARD_LASER, assertInputAllowed: true);
    }

    [Test]
    public void WhiteboardToolTouchInteractorDoesNotMoveWallElementIfDrawModeActive() {
      MockHitElement(true);
      MockToolType(WhiteboardToolStateTypes.DRAWING);
      // We instead let user implicitly draw on the element
      AssertInput(WALL, InputType.CANVAS_ELEMENTS, ToolType.WHITEBOARD_TOUCH, assertInputAllowed: false);
    }

    [Test]
    public void WhiteboardToolTouchInteractorMovesWallElementIfSelectModeActive() {
      MockHitElement(true);
      MockToolType(WhiteboardToolStateTypes.SELECT);
      AssertInput(WALL, InputType.CANVAS_ELEMENTS, ToolType.WHITEBOARD_TOUCH, assertInputAllowed: true);
    }

    [Test]
    public void WhiteboardToolTouchInteractorMovesDeskElementInAnyMode() {
      MockHitElement(true);
      MockToolType(WhiteboardToolStateTypes.SELECT);
      AssertInput(DESK, InputType.CANVAS_ELEMENTS, ToolType.WHITEBOARD_TOUCH, assertInputAllowed: true);
      // Sticky note too small to draw on at desk, we want them to move instead
      MockToolType(WhiteboardToolStateTypes.DRAWING);
      AssertInput(DESK, InputType.CANVAS_ELEMENTS, ToolType.WHITEBOARD_TOUCH, assertInputAllowed: true);
    }

    [Test]
    public void WhiteboardToolInteractorCanBeDisabledByCanvasLoading() {
      MockHitElement(true);

      canvasDriver_.LoadingState.IsLoading.Returns(false);
      MockToolType(WhiteboardToolStateTypes.SELECT);
      AssertInput(DESK, InputType.CANVAS_ELEMENTS, ToolType.WHITEBOARD_TOUCH, assertInputAllowed: true);
      AssertInput(DESK, InputType.CANVAS_ELEMENTS, ToolType.WHITEBOARD_LASER, assertInputAllowed: true);
      MockToolType(WhiteboardToolStateTypes.DRAWING);
      AssertInput(DESK, InputType.WHITEBOARD, ToolType.WHITEBOARD_TOUCH, assertInputAllowed: true);
      AssertInput(DESK, InputType.WHITEBOARD, ToolType.WHITEBOARD_LASER, assertInputAllowed: true);

      canvasDriver_.LoadingState.IsLoading.Returns(true);
      MockToolType(WhiteboardToolStateTypes.SELECT);
      AssertInput(DESK, InputType.CANVAS_ELEMENTS, ToolType.WHITEBOARD_TOUCH, assertInputAllowed: false);
      AssertInput(DESK, InputType.CANVAS_ELEMENTS, ToolType.WHITEBOARD_LASER, assertInputAllowed: false);
      MockToolType(WhiteboardToolStateTypes.DRAWING);
      AssertInput(DESK, InputType.WHITEBOARD, ToolType.WHITEBOARD_TOUCH, assertInputAllowed: false);
      AssertInput(DESK, InputType.WHITEBOARD, ToolType.WHITEBOARD_LASER, assertInputAllowed: false);
    }

    [Test]
    public void WallInputNotAllowedFromDesk() {
      AssertInput(
        WALL,
        InputType.CANVAS_ELEMENTS,
        ToolType.DEFAULT_LASER,
        overridePlayerLocation: DESK,
        assertInputAllowed: false
      );
      AssertInput(
        WALL,
        InputType.CANVAS_ELEMENTS,
        ToolType.DEFAULT_LASER,
        overridePlayerLocation: WALL,
        assertInputAllowed: true
      );
    }


    [Test]
    public void DraggingOnElementAtDeskInDesktopModeWithDrawingToolMovesElement() {
      MockToolType(WhiteboardToolStateTypes.DRAWING);
      // Mock desktop mode
      devicePlatform_.IsVREnabled().Returns(false);
      // Mock element being hit
      MockHitElement(true);
      // Assert canvas element input is allowed at desk -> element should be dragged
      AssertInput(DESK, InputType.CANVAS_ELEMENTS, ToolType.DEFAULT_LASER, assertInputAllowed: true);
      // Assert canvas element input not allowed at wall -> instead we start whiteboarding
      AssertInput(WALL, InputType.CANVAS_ELEMENTS, ToolType.DEFAULT_LASER, assertInputAllowed: false);
    }

    [Test]
    public void ForwardLaserCanPanInSelectModeInDesktopMode() {
      MockHitElement(false);
      // Mock desktop mode
      devicePlatform_.IsVREnabled().Returns(false);
      MockToolType(WhiteboardToolStateTypes.SELECT);
      AssertInput(DESK, InputType.CANVAS_ELEMENTS, ToolType.DEFAULT_LASER, assertInputAllowed: true);
    }

    [Test]
    public void DrawingInputNotAllowedInKeyboardMode() {
      MockToolType(WhiteboardToolStateTypes.KEYBOARD);
      AssertInput(DESK, InputType.WHITEBOARD, ToolType.WHITEBOARD_TOUCH, assertInputAllowed: false);
    }
    
    private void MockHitElement(bool isHit) {
      var hit = isHit ? Substitute.For<ICanvasElement>() : null;
      canvasInputController_.GetHitElement(default).ReturnsForAnyArgs((hit, null));
      canvasInputController_.GetHitElementForWorldPosition(default).ReturnsForAnyArgs((hit, null));
    }

    private void MockToolType(WhiteboardToolStateTypes toolType) {
      whiteboardToolsStateService_.ToolsState.Returns(new ToolsState {ActiveTool = toolType});
    }

    private void AssertInput(
      WorkroomsLocation canvasLocation,
      InputType inputType,
      ToolType toolType,
      bool assertInputAllowed,
      WorkroomsLocation? overridePlayerLocation = null
    ) {
      localPlayerSurfaceAnchorController_.LocalPlayerAnchorLocation.Returns(overridePlayerLocation ?? canvasLocation);
      canvasInstance_.Location.Returns(canvasLocation);
      var filter = canvasLocation == DESK ? deskAllowedInputFilter_ : wallAllowedInputFilter_;
      var interactor = TypeToInteractor(toolType);
      Assert.AreEqual(
        assertInputAllowed,
        inputType == InputType.WHITEBOARD
          ? filter.AllowWhiteboardDrawing(interactor)
          : filter.AllowCanvasElementInput(interactor)
      );
    }

    private IInputInteractor TypeToInteractor(ToolType toolType) {
      switch (toolType) {
        case ToolType.WHITEBOARD_TOUCH: return whiteboardToolTouchInteractor_;
        case ToolType.WHITEBOARD_LASER:
          return whiteboardToolLaserInteractor_;
        case ToolType.DEFAULT_LASER:
          return defaultLaserInteractor_;
        default:
          throw new ArgumentOutOfRangeException(nameof(toolType), toolType, null);
      }
    }

    private enum InputType {
      WHITEBOARD,
      CANVAS_ELEMENTS
    }

    private enum ToolType {
      WHITEBOARD_TOUCH,
      WHITEBOARD_LASER,
      DEFAULT_LASER,
    }
  }
}
