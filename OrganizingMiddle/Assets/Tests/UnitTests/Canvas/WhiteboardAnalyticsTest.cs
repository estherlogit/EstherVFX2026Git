// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using Facebook.SocialVR.Core.Analytics;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Analytics.Events.Whiteboard;
using Facebook.Workrooms.Scene;
using Facebook.Workrooms.ScreenSharing;
using Facebook.Workrooms.Surfaces;
using Facebook.Workrooms.Testing;
using Facebook.Workrooms.Whiteboard;
using Facebook.Xplat.Analytics;
using Facebook.Xplat.Events;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.SceneManagement;

namespace Facebook.Workrooms.Tests.UnitTests.Whiteboard {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class WhiteboardAnalyticsTest {
    private WhiteboardAnalyticsController whiteboardAnalytics_;
    private DispatcherSpyDecorator dispatcher_;
    private IServiceContainer container_;
    private IAnalytics analyticsService_;
    private IQPLLogger qplLoggerService_;
    private FakeLocalPlayerSurfaceAnchorController localPlayerSurfaceAnchorController_;
    private FakeMainScreenController mainScreenController_;

    [SetUp]
    public void Setup() {
      // set up dependencies
      var serviceContainer = ServiceLocator.RootContainer;
      dispatcher_ = new DispatcherSpyDecorator(new Dispatcher(), Substitute.For<IDispatcher>());
      serviceContainer.Bind<IDispatcher>().To(dispatcher_);

      container_ = ServiceLocator.CreateServiceContainer(SceneManager.GetActiveScene().path);
    }

    [TearDown]
    public void Cleanup() {
      ServiceLocator.Clear();
      container_.Clear();
      ServiceLocator.RemoveServiceContainer(SceneManager.GetActiveScene().path);
    }

    private void InitTestClasses() {
      analyticsService_ = Substitute.For<IAnalytics>();
      qplLoggerService_ = Substitute.For<IQPLLogger>();
      localPlayerSurfaceAnchorController_ = new FakeLocalPlayerSurfaceAnchorController();
      mainScreenController_ = new FakeMainScreenController();
      whiteboardAnalytics_ = new WhiteboardAnalyticsController(
        dispatcher_,
        localPlayerSurfaceAnchorController_,
        mainScreenController_,
        analyticsService_,
        qplLoggerService_
      );
    }

    [Test]
    public void WhiteboardingStartedAnalyticsEventDispatchedWhenLocalPlayerAnchorLocationChangedEventReceived() {
      InitTestClasses();
      localPlayerSurfaceAnchorController_.LocalPlayerAnchorLocation = WorkroomsLocation.WALL;
      mainScreenController_.ViewType = MainScreenViewType.WHITEBOARD;
      dispatcher_.Dispatch(new LocalPlayerAnchorLocationChangedEvent());
      analyticsService_.Received().Log(Arg.Is<WhiteboardingStartedAnalyticsEvent>(e => true));
    }

    [Test]
    public void WhiteboardingStartedAnalyticsEventDispatchedWhenMainScreenViewTypeChangedEventReceived() {
      InitTestClasses();
      localPlayerSurfaceAnchorController_.LocalPlayerAnchorLocation = WorkroomsLocation.WALL;
      mainScreenController_.ViewType = MainScreenViewType.WHITEBOARD;
      dispatcher_.Dispatch(new MainScreenViewTypeChangedEvent());
      analyticsService_.Received().Log(Arg.Is<WhiteboardingStartedAnalyticsEvent>(e => true));
    }

    [Test]
    public void WhiteboardingStartedAnalyticsEventNotDispatchedWhenMainScreenViewTypeIsScreenShare() {
      InitTestClasses();
      localPlayerSurfaceAnchorController_.LocalPlayerAnchorLocation = WorkroomsLocation.WALL;
      mainScreenController_.ViewType = MainScreenViewType.SCREEN_SHARING;
      dispatcher_.Dispatch(new MainScreenViewTypeChangedEvent());
      analyticsService_.DidNotReceive().Log(Arg.Is<WhiteboardingStartedAnalyticsEvent>(e => true));
    }

    [Test]
    public void WhiteboardingStoppedAnalyticsEventDispatchedWhenLocalPlayerAnchorLocationChangedEventChangesToDesk() {
      InitTestClasses();
      localPlayerSurfaceAnchorController_.LocalPlayerAnchorLocation = WorkroomsLocation.WALL;
      mainScreenController_.ViewType = MainScreenViewType.WHITEBOARD;
      dispatcher_.Dispatch(new LocalPlayerAnchorLocationChangedEvent());

      localPlayerSurfaceAnchorController_.LocalPlayerAnchorLocation = WorkroomsLocation.DESK;
      dispatcher_.Dispatch(new LocalPlayerAnchorLocationChangedEvent());

      analyticsService_.Received().Log(Arg.Is<WhiteboardingStoppedAnalyticsEvent>(e => true));
    }

    [OnCall(OnCallName.workrooms_creative_collaborations)]
    class FakeMainScreenController : IMainScreenController {
      public MainScreenViewType ViewType { get; set; }
#pragma warning disable CS0067 // Disable unused event error
      public event Action ViewTypeChanged;
#pragma warning restore CS0067
    }
  }
}
