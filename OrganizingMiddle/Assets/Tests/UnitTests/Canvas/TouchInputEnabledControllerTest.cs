// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using Facebook.SocialVR.Core.Inputs;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.Surfaces;
using Facebook.Workrooms.Testing;
using Facebook.Workrooms.Whiteboard;
using Facebook.Workrooms.WhiteboardTool;
using Facebook.Xplat.Analytics;
using Facebook.Xplat.Events;
using NSubstitute;
using NUnit.Framework;

namespace Facebook.Workrooms.Tests.UnitTests.Whiteboard {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class TouchInputEnabledControllerTest {
    private ILocalPlayerSurfaceAnchorController localPlayerSurfaceAnchorController_;
    private ToolAutoSelectionController autoSelectionController_;
    private DispatcherSpyDecorator dispatcher_;
    private ICanvasConfig canvasConfig_;

    private TouchEnabledController touchEnabledController_;

    [SetUp]
    public void Setup() {
      localPlayerSurfaceAnchorController_ = Substitute.For<ILocalPlayerSurfaceAnchorController>();

      dispatcher_ = new DispatcherSpyDecorator(new Dispatcher(), Substitute.For<IDispatcher>());

      canvasConfig_ = Substitute.For<ICanvasConfig>();

      touchEnabledController_ = new TouchEnabledController(
        dispatcher_,
        canvasConfig_,
        localPlayerSurfaceAnchorController_,
        new ControllerSensorDetector(dispatcher_, Substitute.For<IAnalytics>())
      );
    }

    [Test]
    public void TouchInputEnabledByDefault() {
      MoveToLocation(WorkroomsLocation.WALL, WorkroomsLocation.DESK);
      Assert.AreEqual(true, touchEnabledController_.TouchInputEnabledForHand(Handedness.LEFT));
      Assert.AreEqual(true, touchEnabledController_.TouchInputEnabledForHand(Handedness.RIGHT));
      MoveToLocation(WorkroomsLocation.DESK, WorkroomsLocation.WALL);
      Assert.AreEqual(true, touchEnabledController_.TouchInputEnabledForHand(Handedness.LEFT));
      Assert.AreEqual(true, touchEnabledController_.TouchInputEnabledForHand(Handedness.RIGHT));
    }

    [Test]
    public void TouchInputCanBeDisabledViaConfig() {
      MoveToLocation(WorkroomsLocation.WALL, WorkroomsLocation.DESK);
      canvasConfig_.DisableTouchInputDesk.Returns(true);
      Assert.AreEqual(false, touchEnabledController_.TouchInputEnabledForHand(Handedness.LEFT));
      Assert.AreEqual(false, touchEnabledController_.TouchInputEnabledForHand(Handedness.RIGHT));

      // Config did not disable touch input at midair whiteboard
      MoveToLocation(WorkroomsLocation.DESK, WorkroomsLocation.WALL);
      SetAnchorType(MixedRealityAnchorType.MIDAIR_WHITEBOARD);
      Assert.AreEqual(true, touchEnabledController_.TouchInputEnabledForHand(Handedness.LEFT));
      Assert.AreEqual(true, touchEnabledController_.TouchInputEnabledForHand(Handedness.RIGHT));

      // Config did disable touch input at physical whiteboard
      SetAnchorType(MixedRealityAnchorType.PHYSICAL_WHITEBOARD);
      Assert.AreEqual(false, touchEnabledController_.TouchInputEnabledForHand(Handedness.LEFT));
      Assert.AreEqual(false, touchEnabledController_.TouchInputEnabledForHand(Handedness.RIGHT));
    }

    [Test]
    public void TouchInputIsDisabledWhenStylusSensorDetected() {
      MoveToLocation(WorkroomsLocation.WALL, WorkroomsLocation.DESK);
      dispatcher_.Dispatch(new WhiteboardMarkerDrawAction() { hand = Handedness.LEFT });
      Assert.AreEqual(false, touchEnabledController_.TouchInputEnabledForHand(Handedness.LEFT));
      // Only left hand is disabled
      Assert.AreEqual(true, touchEnabledController_.TouchInputEnabledForHand(Handedness.RIGHT));

      // Stylus can't be used at midair whiteboard, so we never disable touch input
      MoveToLocation(WorkroomsLocation.DESK, WorkroomsLocation.WALL);
      SetAnchorType(MixedRealityAnchorType.MIDAIR_WHITEBOARD);
      Assert.AreEqual(true, touchEnabledController_.TouchInputEnabledForHand(Handedness.LEFT));
      Assert.AreEqual(true, touchEnabledController_.TouchInputEnabledForHand(Handedness.RIGHT));

      // Stylus can be used for physical whiteboard
      SetAnchorType(MixedRealityAnchorType.PHYSICAL_WHITEBOARD);
      Assert.AreEqual(false, touchEnabledController_.TouchInputEnabledForHand(Handedness.LEFT));
      // Only left hand is disabled
      Assert.AreEqual(true, touchEnabledController_.TouchInputEnabledForHand(Handedness.RIGHT));
    }

    private void MoveToLocation(WorkroomsLocation fromLoc, WorkroomsLocation newLoc) {
      localPlayerSurfaceAnchorController_.LocalPlayerAnchorLocation.Returns(newLoc);
      dispatcher_.Dispatch(
        new LocalPlayerAnchorLocationChangedEvent() {
          OldAnchorLocation = fromLoc,
          NewAnchorLocation = newLoc
        }
      );
    }

    private void SetAnchorType(MixedRealityAnchorType type) {
      localPlayerSurfaceAnchorController_.LocalPlayerAnchorType.Returns(type);
    }
  }
}
