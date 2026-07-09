// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections.Generic;
using System.Reflection;
using Facebook.SocialVR.Core.Layers;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using Facebook.SocialVR.Core.Player;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.LaserPointer.Config;
using Facebook.Workrooms.Navigation;
using Facebook.Workrooms.ScreenSharing;
using Facebook.Workrooms.Surfaces;
using Facebook.Workrooms.Testing;
using Facebook.Workrooms.TouchInputInteractor;
using Facebook.Workrooms.Whiteboard;
using Facebook.Xplat.Analytics;
using Facebook.Xplat.Events;
using JetBrains.Annotations;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using Assert = UnityEngine.Assertions.Assert;
using Object = System.Object;
using Vector3 = UnityEngine.Vector3;

namespace Facebook.Workrooms.Tests.UnitTests.Whiteboard {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class WhiteboardPopupControllerTest {

    private const float WALL_WIDTH = 6f;
    private const float POPUP_SIZE = 1f;

    private DispatcherSpyDecorator dispatcher_;
    private IServiceContainer container_;
    private GameObject popupParent_;
    private GameObject popupGameObject_;
    private GameObject head_;
    private IPlayerDriver playerDriver_;

    [SetUp]
    public void Setup() {
      ServiceLocator.Clear();
      container_ = ServiceLocator.RootContainer;
      dispatcher_ = new DispatcherSpyDecorator(new Dispatcher(), Substitute.For<IDispatcher>());
      container_.Bind<IDispatcher>().To(dispatcher_);

      popupParent_ = new GameObject();
      popupGameObject_ = new GameObject();
      popupGameObject_.transform.parent = popupParent_.transform;
      popupGameObject_.AddComponent<MeshRenderer>();
      popupGameObject_.GetComponent<MeshRenderer>().bounds = new Bounds(Vector3.zero, new Vector3(POPUP_SIZE, 0f, 0f));

      head_ = new GameObject();
      playerDriver_ = Substitute.For<IPlayerDriver>();
      playerDriver_.head.Returns(head_.transform);

      var logger = Substitute.For<ILogService>();
      container_.Bind<ILogService>().To(logger);
    }

    private void SetPrivateField(Object controller, string fieldName, object fieldValue) {
      var property = controller.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
      property?.SetValue(controller, fieldValue);
    }

    [TearDown]
    public void Cleanup() {
      UnityEngine.Object.DestroyImmediate(popupParent_);
      ServiceLocator.Clear();
      container_.Clear();
      ServiceLocator.RemoveServiceContainer(SceneManager.GetActiveScene().path);
    }

    [Test]
    public void VisiblePopupChangesWhenShowEventReceived() {
      var popupController = CreatePopupController();

      ShowPopup(PopupType.FLIP_CONTROLLER);

      dispatcher_.Spy.Received()
        .Dispatch(Arg.Is<VisibleWhiteboardPopupChangedEvent>(e => e.NewPopupType == PopupType.FLIP_CONTROLLER));
    }

    [Test]
    public void HigherPriorityPopupHidesLowerPriority() {
      var popupController = CreatePopupController();

      ShowPopup(PopupType.FLIP_CONTROLLER);
      Assert.AreEqual(PopupType.FLIP_CONTROLLER, popupController.VisiblePopupType);

      ShowPopup(PopupType.USE_CONTROLLER);
      // Flip controller popup is lower priority, no longer visible
      Assert.AreEqual(PopupType.USE_CONTROLLER, popupController.VisiblePopupType);

      HidePopup(PopupType.USE_CONTROLLER);
      // Flip controller is shown when use controller popup hides
      Assert.AreEqual(PopupType.FLIP_CONTROLLER, popupController.VisiblePopupType);
    }

    [Test]
    public void LowerPriorityPopupDoesNotHideHigherPriority() {
      var popupController = CreatePopupController();

      ShowPopup(PopupType.USE_CONTROLLER);
      Assert.AreEqual(PopupType.USE_CONTROLLER, popupController.VisiblePopupType);

      ShowPopup(PopupType.FLIP_CONTROLLER);
      // Flip controller popup is lower priority, use controller remains visible
      Assert.AreEqual(PopupType.USE_CONTROLLER, popupController.VisiblePopupType);

      HidePopup(PopupType.USE_CONTROLLER);
      // Flip controller is shown when use controller popup hides
      Assert.AreEqual(PopupType.FLIP_CONTROLLER, popupController.VisiblePopupType);
    }

    [Test]
    public void PopupChangedEventDoesNotReportUndefinedPopupWhenSwitchingToQueuedPopup() {
      var popupController = CreatePopupController();

      ShowPopup(PopupType.CANVAS_SETUP);
      ShowPopup(PopupType.FLIP_CONTROLLER);
      HidePopup(PopupType.CANVAS_SETUP);

      // UNDEFINED not received when switching directly to flipped controller popup
      dispatcher_.Spy.DidNotReceive()
        .Dispatch(Arg.Is<VisibleWhiteboardPopupChangedEvent>(e => e.NewPopupType == PopupType.UNDEFINED));

      HidePopup(PopupType.FLIP_CONTROLLER);
      // No queued popups, now trigger change to UNDEFINED popup event
      dispatcher_.Spy.Received()
        .Dispatch(Arg.Is<VisibleWhiteboardPopupChangedEvent>(e => e.NewPopupType == PopupType.UNDEFINED));
    }

    [Test]
    public void WhiteboardDrawingEventDismissesDissmissablePopup() {
      var popupController = CreatePopupController();
      ShowPopup(PopupType.DISMISSABLE_POPUP_UNIT_TEST);
      dispatcher_.Dispatch(new WhiteboardDrawingStartedEvent());
      Assert.AreEqual(PopupType.UNDEFINED, popupController.VisiblePopupType);
    }

    [Test]
    public void WhiteboardDrawingEventDoesNotDismissFlipControllerPopup() {
      var popupController = CreatePopupController();
      ShowPopup(PopupType.FLIP_CONTROLLER);
      dispatcher_.Dispatch(new WhiteboardDrawingStartedEvent());
      Assert.AreEqual(PopupType.FLIP_CONTROLLER, popupController.VisiblePopupType);
    }

    [Test]
    public void CollisionInputOnPopupDismissesLanyardPopup() {
      var popupController = CreatePopupController();
      ShowPopup(PopupType.DISMISSABLE_POPUP_UNIT_TEST);
      dispatcher_.Dispatch(new TouchInputInteractorEnterEvent() { Source = popupGameObject_ });
      Assert.AreEqual(PopupType.UNDEFINED, popupController.VisiblePopupType);
    }

    [Test]
    public void CollisionInputOnPopupDoesNotDismissFlipControllerPopup() {
      var popupController = CreatePopupController();
      ShowPopup(PopupType.FLIP_CONTROLLER);
      dispatcher_.Dispatch(new TouchInputInteractorEnterEvent() { Source = popupGameObject_ });
      Assert.AreEqual(PopupType.FLIP_CONTROLLER, popupController.VisiblePopupType);
    }

    [Test]
    public void WallPopupIsPlacedAtPlayerXPosition() {
      // Apply offset to parent to ensure tracking still works when player is at different wall positions
      popupParent_.transform.position = new Vector3(-2, 0, 0);

      var popupController = CreatePopupController();
      popupController.TrackPlayerPositionForWallPopup = true;

      // Player x position is within the whiteboard
      // Popup is placed at the same position
      head_.transform.position = new Vector3(1.5f, 0f, 0f);
      ShowPopup(PopupType.STICKY_NOTES_NUX);
      Assert.AreEqual(popupGameObject_.transform.position, head_.transform.position);
      HidePopup(PopupType.STICKY_NOTES_NUX);

      // Player x position is far past the right edge of the whiteboard
      // Popup is placed at the right edge of the whiteboard
      head_.transform.position = new Vector3(1000f, 0f, 0f);
      ShowPopup(PopupType.STICKY_NOTES_NUX);
      float rightEdge = (WALL_WIDTH - POPUP_SIZE) / 2;
      Assert.AreEqual(popupGameObject_.transform.position, new Vector3(rightEdge, 0f, 0f));
      HidePopup(PopupType.STICKY_NOTES_NUX);

      // Player x position is far past the left edge of the whiteboard
      // Popup is placed at the left edge of the whiteboard
      head_.transform.position = new Vector3(-1000f, 0f, 0f);
      ShowPopup(PopupType.STICKY_NOTES_NUX);
      float leftEdge = -(WALL_WIDTH - POPUP_SIZE) / 2;
      Assert.AreEqual(popupGameObject_.transform.position, new Vector3(leftEdge, 0f, 0f));
      HidePopup(PopupType.STICKY_NOTES_NUX);
    }

    [Test]
    public void CanvasSetupPopupIsLowerPriorityThanOnlyRecalibrationAndControllerPopups() {
      // This test is to prevent new popup types of higher priority being added which block the canvas setup popup
      var popupController = CreatePopupController();

      var higherPriorityThanCanvasSetup = new HashSet<PopupType>() {
        // May need to recalibrate during canvas setup
        PopupType.RECALIBRATION,
        // May need to instruct user to use controller during canvas setup
        PopupType.USE_CONTROLLER,
        // Ignore other canvas setup popup types
        PopupType.CANVAS_SETUP_LONG,
        PopupType.CANVAS_SETUP_INTRO
      };

      ShowPopup(PopupType.CANVAS_SETUP);
      Assert.AreEqual(PopupType.CANVAS_SETUP, popupController.VisiblePopupType);

      foreach (PopupType popupType in Enum.GetValues(typeof(PopupType))) {
        if (higherPriorityThanCanvasSetup.Contains(popupType)) {
          // Skip because this is one which can interrupt the canvas setup
          continue;
        }

        // Ensure showing this popup doesn't dismiss the canvas setup popup
        ShowPopup(popupType);
        Assert.AreEqual(PopupType.CANVAS_SETUP, popupController.VisiblePopupType);
      }
    }

    [Test]
    public void MultipleActiveVideoPopupsKeepVideoPlayerActiveOnVisiblePopupChanged() {
      var videoPlayer = new MockVideoPlayer();
      CreatePopupController(videoPlayer);
      ShowPopup(PopupType.FLIP_CONTROLLER, hasVideoClip: true);
      // Video player being used by 1st popup
      Assert.AreEqual(true, videoPlayer.IsActive);
      ShowPopup(PopupType.STICKY_NOTES_NUX, hasVideoClip: true);
      // Still being used for 1st popup
      Assert.AreEqual(true, videoPlayer.IsActive);
      HidePopup(PopupType.FLIP_CONTROLLER);
      // Video player now used for 2nd popup
      Assert.AreEqual(true, videoPlayer.IsActive);
      HidePopup(PopupType.STICKY_NOTES_NUX);
      // All popups hidden
      Assert.AreEqual(false, videoPlayer.IsActive);
    }

    private void ShowPopup(PopupType type, bool hasVideoClip = false) {
      dispatcher_.Dispatch(
        new WhiteboardPopupShowEvent() {
          EventArgs = new WhiteboardPopupShowArgs() {
            PopupType = type,
            PopupAnchorType = PopupAnchorType.WALL,
            HasVideoClip = hasVideoClip
          }
        }
      );
    }

    private void HidePopup(PopupType type) {
      dispatcher_.Dispatch(new WhiteboardPopupHideEvent(type, PopupAnchorType.WALL));
    }

    private WhiteboardPopupController CreatePopupController([CanBeNull] IWorkroomsVideoPlayer videoPlayer = null) {
      videoPlayer ??= Substitute.For<IWorkroomsVideoPlayer>();
      var localPlayerSurfaceAnchorController = Substitute.For<ILocalPlayerSurfaceAnchorController>();
      localPlayerSurfaceAnchorController.LocalPlayerAnchorLocation.Returns(WorkroomsLocation.WALL);
      var popupController = new WhiteboardPopupController(
        popupGameObject_.transform,
        PopupAnchorType.WALL,
        popupGameObject_.transform,
        popupGameObject_,
        dispatcher_,
        Substitute.For<INavigationService>(),
        localPlayerSurfaceAnchorController,
        Substitute.For<DynamicLayerPicker>(),
        videoPlayer,
        popupGameObject_.GetComponent<MeshRenderer>(),
        Substitute.For<IAnalytics>(),
        Substitute.For<CanvasZPositions>(),
        WALL_WIDTH,
        Substitute.For<IMainScreenController>(),
        playerDriver_
      );
      popupController.PopupUIInitialized();
      return popupController;
    }

    private class MockVideoPlayer : IWorkroomsVideoPlayer {

#pragma warning disable CS0067 // Disable unused event error
      public event Action VideoPrepared;
      public event Action VideoPreparationStarted;
#pragma warning restore CS0067

      public VideoClip clip { get; set; }
      public bool IsActive { get; private set; }

      public void SetActive(bool active) {
        IsActive = active;
      }

      public bool IsPrepared { get; }
    }
  }
}
