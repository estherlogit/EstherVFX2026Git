// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using Facebook.SocialVR.Core.Bedrock;
using Facebook.SocialVR.Core.Inputs;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using Facebook.SocialVR.Modules.CodeGenPrefabs;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.Canvas.SetupSequence;
using Facebook.Workrooms.InputDevice;
using Facebook.Workrooms.Surfaces;
using Facebook.Workrooms.Testing;
using Facebook.Workrooms.Whiteboard;
using Facebook.Workrooms.WhiteboardTool;
using Facebook.Xplat.Events;
using NSubstitute;
using NUnit.Framework;
using ReactVR;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Facebook.Workrooms.Tests.UnitTests.Whiteboard {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class WhiteboardToolsServiceTest {
    private WhiteboardToolsService controller_;
    private WhiteboardToolsStateService whiteboardToolsStateService_;
    private IWhiteboardTool leftTool_;
    private IWhiteboardTool rightTool_;
    private GameObject testRoot_;
    private DispatcherSpyDecorator dispatcher_;
    private IServiceContainer container_;

    [SetUp]
    public void Setup() {
      // set up dependencies
      var serviceContainer = ServiceLocator.RootContainer;
      dispatcher_ = new DispatcherSpyDecorator(new Dispatcher(), Substitute.For<IDispatcher>());
      serviceContainer.Bind<IDispatcher>().To(dispatcher_);
      serviceContainer.Bind<ILogService>().To(Substitute.For<ILogService>());
      whiteboardToolsStateService_ = new WhiteboardToolsStateService(dispatcher_);

      // init mb
      testRoot_ = new GameObject();
      testRoot_.SetActive(false);
      leftTool_ = Substitute.For<IWhiteboardTool>();
      var leftToolGameObject = (new GameObject());
      leftTool_.gameObject.Returns(leftToolGameObject);
      leftTool_.gameObject.transform.SetParent(testRoot_.transform);
      var rightToolGameObject = (new GameObject());
      rightTool_ = Substitute.For<IWhiteboardTool>();
      rightTool_.gameObject.Returns(rightToolGameObject);
      rightTool_.gameObject.transform.SetParent(testRoot_.transform);
      controller_ = new WhiteboardToolsService(
        dispatcher_,
        Substitute.For<ILocalPlayerSurfaceAnchorController>(),
        Substitute.For<ICanvasInstanceService>(),
        Substitute.For<ICanvasSetupService>(),
        Substitute.For<IInputMethodService>(),
        whiteboardToolsStateService_,
        Substitute.For<IDevicePlatform>(),
        Substitute.For<IMixedRealityAnchorService>(),
        leftTool_,
        rightTool_,
        Substitute.For<IServicesMonoBehaviourManager>(),
        Substitute.For<CodeGenPrefabsMap>(),
        Substitute.For<ILogService>()
      );
      container_ = ServiceLocator.CreateServiceContainer(SceneManager.GetActiveScene().path);
    }

    [TearDown]
    public void Cleanup() {
      GameObject.DestroyImmediate(testRoot_);
      ServiceLocator.Clear();
      container_.Clear();
      ServiceLocator.RemoveServiceContainer(SceneManager.GetActiveScene().path);
    }

    [Test]
    public void SetActiveToolDispatchesStateChangeEvent() {
      controller_.HandleReactToolStateChange(WhiteboardToolStateTypes.ERASER, 1, "black");

      // we can access the spy to see which events have been fired
      dispatcher_.Spy.Received().Dispatch(Arg.Is<WhiteboardToolsStateChangedEvent>(e => true));
    }

    [Test]
    public void SetActiveToolChangesState() {
      controller_.HandleReactToolStateChange(WhiteboardToolStateTypes.DRAWING, 5, "red");

      Assert.True(
        whiteboardToolsStateService_.ToolsState.ActiveTool == WhiteboardToolStateTypes.DRAWING
        && whiteboardToolsStateService_.ToolsState.Thickness == 5
        && whiteboardToolsStateService_.ToolsState.SelectedColor == Color.red,
        "Expected active tool state to be set"
      );
    }

    [Test]
    public void TestWhiteboardDrawingStartedEventAndSetActiveToolCall() {
      // Dispatch event that would normally come via WhiteboardTool when touching surface
      var whiteboardDrawingReactPanel = Substitute.For<IWhiteboardReactPanel>();
      whiteboardDrawingReactPanel.DrawingsType.Returns(BaseWhiteboardModule.WhiteboardDrawingsType.MAIN_WHITEBOARD);
      dispatcher_.Dispatch(
        (new WhiteboardDrawingStartedEvent()).Init(whiteboardDrawingReactPanel.DrawingsType, Handedness.LEFT, WorkroomsLocation.WALL)
      );
      // Ensure this call doesn't clear drawing state
      controller_.HandleReactToolStateChange(WhiteboardToolStateTypes.DRAWING, 1, "red");

      Assert.True(whiteboardToolsStateService_.ToolsState.IsLeftToolDrawing, "Left tool should be drawing");
      Assert.True(
        whiteboardToolsStateService_.ToolsState.IsRightToolDrawing == false,
        "Right tool should be not be drawing"
      );
    }

    [Test]
    public void TestWhiteboardToolAttachEventAndSetActiveToolCall() {
      dispatcher_.Dispatch(new WhiteboardToolAttachEvent() {Source = rightTool_.gameObject});
      // Ensure this call doesn't clear active state
      controller_.HandleReactToolStateChange(WhiteboardToolStateTypes.DRAWING, 1, "red");

      Assert.True(whiteboardToolsStateService_.ToolsState.IsLeftToolActive == false, "Left tool should be inactive");
      Assert.True(whiteboardToolsStateService_.ToolsState.IsRightToolActive, "Right tool should be active");
    }
  }
}
