// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using Facebook.SocialVR.Core.Inputs;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.Utils;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.InputDevice;
using Facebook.Workrooms.Surfaces;
using Facebook.Workrooms.Testing;
using Facebook.Xplat.Events;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using Facebook.SocialVR.Core.OnCall;

namespace Facebook.Workrooms.Tests.UnitTests.Whiteboard {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class WhiteboardNUXPopupControllerTest {
    private DispatcherSpyDecorator dispatcher_;
    private IServiceContainer container_;
    private WhiteboardNUXPopupController whiteboardNUXPopupController_;
    private ILocalPlayerSurfaceAnchorController localPlayerSurfaceAnchorController_;
    private IInputMethodService inputMethodService_;
    private IPlayerPrefsInt nuxCompletionCounter_;
    private HandedInputMethod fakeInputMethod_;

    [SetUp]
    public void Setup() {
      container_ = ServiceLocator.CreateServiceContainer(SceneManager.GetActiveScene().path);
      dispatcher_ = new DispatcherSpyDecorator(new Dispatcher(), Substitute.For<IDispatcher>());
      container_.Bind<IDispatcher>().To(dispatcher_);

      inputMethodService_ = Substitute.For<IInputMethodService>();

      var logger = Substitute.For<ILogService>();
      container_.Bind<ILogService>().To(logger);
    }

    [TearDown]
    public void Cleanup() {
      ServiceLocator.Clear();
      container_.Clear();
      ServiceLocator.RemoveServiceContainer(SceneManager.GetActiveScene().path);
    }

    [Test]
    public void ShowFlipControllerNUXIfInputTypeController() {
      ReinitTestClasses(WorkroomsLocation.WALL);
      float time = 0;
      fakeInputMethod_.InputMethod = InputMethod.CONTROLLER;

      // Not shown until before initial delay
      whiteboardNUXPopupController_.UpdateState(time);

      // After delay, controller popup shown
      time += WhiteboardNUXPopupController.PopupShowDelay;
      whiteboardNUXPopupController_.UpdateState(time);
      dispatcher_.Spy.Received()
        .Dispatch(Arg.Is<WhiteboardPopupShowEvent>(e => e.EventArgs.PopupType == PopupType.FLIP_CONTROLLER));

      // Do not hide flipped controller nux until anims completes (TIME_TO_SHOW_FLIPPED_CONTROLLER_NUX)
      fakeInputMethod_.InputMethod = InputMethod.FLIPPED_CONTROLLER;
      whiteboardNUXPopupController_.UpdateState(time);
      dispatcher_.Spy.DidNotReceive()
        .Dispatch(Arg.Is<WhiteboardPopupHideEvent>(e => e.PopupType == PopupType.FLIP_CONTROLLER));

      // Hide controller when controller flipped after showing for at least TIME_TO_SHOW_FLIPPED_CONTROLLER_NUX
      time += WhiteboardNUXPopupController.TimeToShowFlippedControllerNUX;
      whiteboardNUXPopupController_.UpdateState(time);
      dispatcher_.Spy.Received()
        .Dispatch(Arg.Is<WhiteboardPopupHideEvent>(e => e.PopupType == PopupType.FLIP_CONTROLLER));
    }

    [Test]
    public void HideFlipControllerNUXAfterTimeout() {
      ReinitTestClasses(WorkroomsLocation.WALL);
      float time = 0;
      fakeInputMethod_.InputMethod = InputMethod.CONTROLLER;

      // Not shown until before initial delay
      whiteboardNUXPopupController_.UpdateState(time);

      // After delay, controller popup shown
      time += WhiteboardNUXPopupController.PopupShowDelay;
      whiteboardNUXPopupController_.UpdateState(time);
      dispatcher_.Spy.Received()
        .Dispatch(Arg.Is<WhiteboardPopupShowEvent>(e => e.EventArgs.PopupType == PopupType.FLIP_CONTROLLER));

      // Hide popup forever after timeout
      time += WhiteboardNUXPopupController.TimeBeforeDismissFlippedControllerNUX;
      whiteboardNUXPopupController_.UpdateState(time);
      dispatcher_.Spy.Received()
        .Dispatch(Arg.Is<WhiteboardPopupHideEvent>(e => e.PopupType == PopupType.FLIP_CONTROLLER));
    }

    private void ReinitTestClasses(WorkroomsLocation location) {
      inputMethodService_ = Substitute.For<IInputMethodService>();
      fakeInputMethod_ = new HandedInputMethod(Handedness.LEFT, InputMethod.NONE, false);
      inputMethodService_.GetHandedInputMethod(Handedness.LEFT).Returns(fakeInputMethod_);
      inputMethodService_.GetHandedInputMethod(Handedness.RIGHT).Returns(fakeInputMethod_);
      inputMethodService_.GetInputMethodType(Handedness.LEFT).Returns((x) => { return fakeInputMethod_.InputMethod; });
      inputMethodService_.GetInputMethodType(Handedness.RIGHT).Returns((x) => { return fakeInputMethod_.InputMethod; });
      inputMethodService_.IsUsingController()
        .Returns(
          (x) => {
            return fakeInputMethod_.InputMethod == InputMethod.CONTROLLER
                   || fakeInputMethod_.InputMethod == InputMethod.FLIPPED_CONTROLLER;
          }
        );
      inputMethodService_.IsUsingOlympusControllers().Returns(false);

      localPlayerSurfaceAnchorController_ = Substitute.For<ILocalPlayerSurfaceAnchorController>();
      localPlayerSurfaceAnchorController_.LocalPlayerAnchorLocation.Returns(location);

      nuxCompletionCounter_ = Substitute.For<IPlayerPrefsInt>();

      whiteboardNUXPopupController_ = new WhiteboardNUXPopupController(
        location,
        nuxCompletionCounter_,
        dispatcher_,
        localPlayerSurfaceAnchorController_,
        inputMethodService_,
        Substitute.For<VideoClipReferences>()
      );
    }
  }
}
