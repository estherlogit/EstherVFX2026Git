// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.DesktopMirror;
using Facebook.Workrooms.Notifications;
using Facebook.Workrooms.Services;
using Facebook.Xplat.Events;
using Facebook.Xplat.Threading;
using NSubstitute;
using NUnit.Framework;

namespace Facebook.Workrooms.Tests.UnitTests {
  [OnCall(OnCallName.workrooms_remote_desktop)]
  public class VirtualDesktopAutoConnectHandlerTest  {
    private VirtualDesktopAutoConnectHandler autoconnectHandler_;
    private IVirtualDesktopConfig config_;
    private IWorkroomsPinCodeService pinCodeService_;

    [SetUp]
    public void Setup() {
      config_ = Substitute.For<IVirtualDesktopConfig>();
      pinCodeService_ = Substitute.For<IWorkroomsPinCodeService>();
      autoconnectHandler_ = new VirtualDesktopAutoConnectHandler(
        config_,
        pinCodeService_,
        new UnityLogService().GetLog("VirtualDesktopAutoConnectHandlerTest"),
        Substitute.For<IDispatcher>(),
        Substitute.For<IMainThreadExecutor>()
      );
      // Default to auto connect  being enable via config and pin code not being required
      config_.IsAutoConnectEnabled().Returns(true);
      pinCodeService_.IsPinCodeRequired().Returns(false);
    }

    [Test]
    public void TestAutoConnectIsAllowedOnFirstTryWhenPinCodeIsNotRequired() {
      Assert.AreEqual(autoconnectHandler_.AutoConnectStatus, AutoConnectStatus.PENDING_FIRST_TRY);
      Assert.IsTrue(autoconnectHandler_.IsAutoconnectAllowed());
    }

    [Test]
    public void TestAutoConnectIsNotAllowedWhenDisabledByConfig() {
      config_.IsAutoConnectEnabled().Returns(false);
      Assert.IsFalse(autoconnectHandler_.IsAutoconnectAllowed());
    }


    [Test]
    public void TestAutoConnectIsNotAllowedWhenPinCodeIsRequired() {
      pinCodeService_.IsPinCodeRequired().Returns(true);
      Assert.IsFalse(autoconnectHandler_.IsAutoconnectAllowed());
    }

    [Test]
    public void TestAutoConnectIsNotAllowedWhenStatusIsInConnectionAttempted() {
      // If we're in CONNECTION_ATTEMPTED we should NOT attempt any further connections
      autoconnectHandler_.HandleAutoConnectAttempt();
      Assert.AreEqual(autoconnectHandler_.AutoConnectStatus, AutoConnectStatus.CONNECTION_ATTEMPTED);
      Assert.IsFalse(autoconnectHandler_.IsAutoconnectAllowed());
    }

    [Test]
    public void TestAutoConnectIsAllowedWhenStatusIsSceneChanged() {
      // Attempt connect
      autoconnectHandler_.HandleAutoConnectAttempt();
      Assert.AreEqual(autoconnectHandler_.AutoConnectStatus, AutoConnectStatus.CONNECTION_ATTEMPTED);

      // Trigger scene change
      autoconnectHandler_.HandleSceneChange();
      Assert.AreEqual(autoconnectHandler_.AutoConnectStatus, AutoConnectStatus.RESET_DUE_TO_SCENE_CHANGE);

      Assert.IsTrue(autoconnectHandler_.IsAutoconnectAllowed());
    }

    [Test]
    public void TestAutoConnectIsAllowedWhenStatusIsRetryAfterDoff() {
      // Attempt connect
      autoconnectHandler_.HandleAutoConnectAttempt();
      Assert.AreEqual(autoconnectHandler_.AutoConnectStatus, AutoConnectStatus.CONNECTION_ATTEMPTED);

      // Doff management
      autoconnectHandler_.HandleDisconnectDueToDoff();
      Assert.AreEqual(autoconnectHandler_.AutoConnectStatus, AutoConnectStatus.RETRY_AFTER_DOFF);

      Assert.IsTrue(autoconnectHandler_.IsAutoconnectAllowed());
    }

    [Test]
    public void TestAutoConnectIsNotAllowedAfterManualDisconnection() {
      // Attempt connect
      autoconnectHandler_.HandleAutoConnectAttempt();
      Assert.AreEqual(autoconnectHandler_.AutoConnectStatus, AutoConnectStatus.CONNECTION_ATTEMPTED);

      // Manual disconnect
      autoconnectHandler_.HandleManualDisconnection();
      Assert.AreEqual(autoconnectHandler_.AutoConnectStatus, AutoConnectStatus.DISABLED_VIA_MANUAL_DISCONNECT);

      // We should not be able to autoconnect after a manual disconnection
      Assert.IsFalse(autoconnectHandler_.IsAutoconnectAllowed());
    }

    [Test]
    public void TestAutoConnectStatusIsNotResetAfterManualDisconnect() {
      // Attempt connect
      autoconnectHandler_.HandleAutoConnectAttempt();
      Assert.AreEqual(autoconnectHandler_.AutoConnectStatus, AutoConnectStatus.CONNECTION_ATTEMPTED);

      // Manual disconnect
      autoconnectHandler_.HandleManualDisconnection();
      Assert.AreEqual(autoconnectHandler_.AutoConnectStatus, AutoConnectStatus.DISABLED_VIA_MANUAL_DISCONNECT);

      // Scene change
      autoconnectHandler_.HandleSceneChange();
      // Status should remain in manual disconnection mode
      Assert.AreEqual(autoconnectHandler_.AutoConnectStatus, AutoConnectStatus.DISABLED_VIA_MANUAL_DISCONNECT);

      // Doff
      autoconnectHandler_.HandleDisconnectDueToDoff();
      // Status should remain in manual disconnection mode
      Assert.AreEqual(autoconnectHandler_.AutoConnectStatus, AutoConnectStatus.DISABLED_VIA_MANUAL_DISCONNECT);

      // We should not be able to autoconnect after a manual disconnection
      Assert.IsFalse(autoconnectHandler_.IsAutoconnectAllowed());
    }
  }
}
