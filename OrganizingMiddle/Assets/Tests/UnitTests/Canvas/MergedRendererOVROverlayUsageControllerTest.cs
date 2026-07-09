// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using Facebook.SocialVR.Core.Bedrock;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Player;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.MixedReality;
using Facebook.Workrooms.Passthrough;
using Facebook.Workrooms.PlayerConfiguration;
using Facebook.Workrooms.Scene;
using Facebook.Workrooms.ScreenSharing;
using Facebook.Workrooms.Surfaces;
using Facebook.Workrooms.Whiteboard;
using Facebook.Xplat.Analytics;
using Facebook.Xplat.Events;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Tests.UnitTests.CollabCanvas {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class MergedRendererOVROverlayUsageControllerTest {
    private MergedRendererOVROverlayUsageController controller_;

    private GameObject testRoot_;
    private GameObject deskCanvas_;
    private GameObject wallCanvas_;
    private GameObject deskWhiteboard_;
    private GameObject wallWhiteboard_;
    private GameObject head_;
    private Camera camera_;

    private CanvasImageRenderingConfig canvasImageRenderingConfig_;
    private IDispatcher dispatcher_;

    private void SetUpObjects() {
      // Setup for the scene: we set up a head with a camera (user), a desk canvas and a wall canvas with its whiteboards. We're mostly interested in their positions so no need to set up more properties.
      testRoot_ = new GameObject();
      testRoot_.SetActive(false);

      deskCanvas_ = new GameObject();
      deskCanvas_.transform.parent = testRoot_.transform;
      deskWhiteboard_ = new GameObject();
      deskWhiteboard_.SetActive(true);
      deskWhiteboard_.transform.parent = deskCanvas_.transform;

      wallCanvas_ = new GameObject();
      wallCanvas_.transform.parent = testRoot_.transform;
      wallWhiteboard_ = new GameObject();
      wallWhiteboard_.SetActive(true);
      wallWhiteboard_.transform.parent = wallCanvas_.transform;

      head_ = new GameObject();
      head_.transform.parent = testRoot_.transform;
      head_.SetActive(true);

      camera_ = head_.AddComponent<Camera>();
      camera_.enabled = true;
      camera_.transform.parent = head_.transform;
      camera_.nearClipPlane = 0.001f;
      camera_.tag = "MainCamera";
    }

    private void SetUpConfig() {
      canvasImageRenderingConfig_ = Substitute.For<CanvasImageRenderingConfig>();
      canvasImageRenderingConfig_.OVROverlayExclusiveDeskBias.Returns(0.1f);
      canvasImageRenderingConfig_.NonOVROverlayWhenDrawing.Returns(false);
      canvasImageRenderingConfig_.OVRMergedImageRenderingDeskInEditor.Returns(true);
      canvasImageRenderingConfig_.OVROverlayExclusive.Returns(true);
    }

    [SetUp]
    public void Setup() {
      SetUpObjects();
      SetUpConfig();

      // Setting up mocks and wiring
      var canvasSegmentsService = Substitute.For<ICanvasInstanceService>();
      var reactPanel = Substitute.For<WhiteboardDrawingReactPanel>();
      canvasSegmentsService.GetWhiteboardDrawingReactPanel().Returns(reactPanel);

      var deskCanvasSegment = Substitute.For<ICanvasInstance>();
      deskCanvasSegment.WhiteboardScaleTransform.Returns(deskCanvas_.transform);
      canvasSegmentsService.GetCanvasInstance(WorkroomsLocation.DESK).Returns(deskCanvasSegment);

      var deskWhiteboardCollider = deskWhiteboard_.AddComponent<BoxCollider>();
      deskWhiteboardCollider.transform.parent = deskWhiteboard_.transform;
      var deskWhiteboardController = Substitute.For<IWhiteboardController>();
      deskWhiteboardController.Collider.Returns(deskWhiteboardCollider);
      deskCanvasSegment.Whiteboard.Returns(deskWhiteboardController);

      var wallCanvasSegment = Substitute.For<ICanvasInstance>();
      wallCanvasSegment.WhiteboardScaleTransform.Returns(wallCanvas_.transform);
      canvasSegmentsService.GetCanvasInstance(WorkroomsLocation.WALL).Returns(wallCanvasSegment);
      var wallWhiteboardCollider = wallWhiteboard_.AddComponent<BoxCollider>();
      wallWhiteboardCollider.transform.parent = wallWhiteboard_.transform;
      var wallWhiteboardController = Substitute.For<IWhiteboardController>();
      wallWhiteboardController.Collider.Returns(wallWhiteboardCollider);
      wallCanvasSegment.Whiteboard.Returns(wallWhiteboardController);

      var playerDriver = Substitute.For<IPlayerDriver>();
      playerDriver.head.Returns(head_.transform);

      dispatcher_ = new Dispatcher();
      controller_ = new MergedRendererOVROverlayUsageController(
        WorkroomsLocation.DESK,
        canvasImageRenderingConfig_,
        dispatcher_,
        canvasSegmentsService,
        Substitute.For<IUpdateRunner>(),
        playerDriver,
        Substitute.For<VCScreenWidget>(),
        Substitute.For<ILogService>(),
        Substitute.For<IDeskWidgetController>(),
        Substitute.For<LocalPlayerSurfaceAnchorController>(),
        Substitute.For<IAnalytics>(),
        Substitute.For<IIsUpdatingStateProvider>(),
        camera_
      );

      timesActionCalled_ = 0;
      controller_.ValuesChanged += ActionCalled;
    }

    private int timesActionCalled_;

    private void ActionCalled() {
      timesActionCalled_++;
    }

    [Test]
    public void WhenDeskIsInFrontWallBehindUseOVRRenderingAndActionIsCalled() {
      deskCanvas_.transform.SetPositionAndRotation(Vector3.forward, Quaternion.identity);
      wallCanvas_.transform.SetPositionAndRotation(Vector3.back, Quaternion.identity);

      controller_.RunUpdate(1);

      Assert.IsFalse(controller_.UseNonOVRRendering);
      Assert.AreEqual(1, timesActionCalled_);
    }

    [Test]
    public void WhenCanvasDimmingIsActiveNoOVRRendering() {
      deskCanvas_.transform.SetPositionAndRotation(Vector3.forward, Quaternion.identity);
      wallCanvas_.transform.SetPositionAndRotation(Vector3.back, Quaternion.identity);

      dispatcher_.Dispatch(new DeskCanvasInstance.DeskCanvasDimmingEvent().Init(true));

      controller_.RunUpdate(1);

      Assert.IsTrue(controller_.UseNonOVRRendering);
    }

    [Test]
    public void WhenPassthroughIsActiveNoOVRRendering() {
      deskCanvas_.transform.SetPositionAndRotation(Vector3.forward, Quaternion.identity);
      wallCanvas_.transform.SetPositionAndRotation(Vector3.back, Quaternion.identity);
      controller_.RunUpdate(1);

      dispatcher_.Dispatch(new PassthroughModeChanged { Mode = PassthroughMode.PARTIAL});
      Assert.IsTrue(controller_.UseNonOVRRendering);

      dispatcher_.Dispatch(new PassthroughModeChanged { Mode = PassthroughMode.FULL});
      Assert.IsTrue(controller_.UseNonOVRRendering);

      dispatcher_.Dispatch(new PassthroughModeChanged { Mode = PassthroughMode.MR_MODE});
      Assert.IsFalse(controller_.UseNonOVRRendering);

      dispatcher_.Dispatch(new PassthroughModeChanged { Mode = PassthroughMode.NONE});
      Assert.IsFalse(controller_.UseNonOVRRendering);
    }

    [Test]
    public void WhenDeskIsBehindWallIsInFrontDontUseOVRRendering() {
      deskCanvas_.transform.SetPositionAndRotation(Vector3.back, Quaternion.identity);
      wallCanvas_.transform.SetPositionAndRotation(Vector3.forward, Quaternion.identity);

      controller_.RunUpdate(1);

      Assert.IsTrue(controller_.UseNonOVRRendering);
    }

    [Test]
    public void WhenWeUpdateTwiceWithoutChangingPositionActionIsCalledOnce() {
      deskCanvas_.transform.SetPositionAndRotation(Vector3.forward, Quaternion.identity);
      wallCanvas_.transform.SetPositionAndRotation(Vector3.back, Quaternion.identity);

      controller_.RunUpdate(1);
      controller_.RunUpdate(2);

      Assert.IsFalse(controller_.UseNonOVRRendering);
      Assert.AreEqual(1, timesActionCalled_);
    }

    [Test]
    public void WhenVisibilityIsSwitchedActionIsCalledTwice() {
      deskCanvas_.transform.SetPositionAndRotation(Vector3.forward, Quaternion.identity);
      wallCanvas_.transform.SetPositionAndRotation(Vector3.back, Quaternion.identity);

      controller_.RunUpdate(1);

      deskCanvas_.transform.SetPositionAndRotation(Vector3.back, Quaternion.identity);
      wallCanvas_.transform.SetPositionAndRotation(Vector3.forward, Quaternion.identity);

      controller_.RunUpdate(2);

      Assert.IsTrue(controller_.UseNonOVRRendering);
      Assert.AreEqual(2, timesActionCalled_);
    }
  }
}
