// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Analytics.Events;
using Facebook.Workrooms.GoogleDrive;
using Facebook.Workrooms.ScreenSharing;
using Facebook.Workrooms.Surfaces;
using Facebook.Workrooms.Testing;
using Facebook.Xplat.Analytics;
using Facebook.Xplat.Events;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using Assert = UnityEngine.Assertions.Assert;

namespace Facebook.Workrooms.Tests.UnitTests {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class MainScreenControllerTest {

    private MainScreenController controller_;
    private DispatcherSpyDecorator dispatcher_;
    private IAnalytics analytics_;
    private int timesUpdateScreenCalled_ = 0;

    [SetUp]
    public void Setup() {
      ServiceLocator.Clear();
      var serviceContainer = ServiceLocator.RootContainer;

      dispatcher_ = new DispatcherSpyDecorator(new Dispatcher(), Substitute.For<IDispatcher>());
      serviceContainer.Bind<IDispatcher>().To(dispatcher_);

      analytics_ = Substitute.For<IAnalytics>();
      serviceContainer.Bind<IAnalytics>().To(analytics_);

      serviceContainer.Bind<ILocalPlayerSurfaceAnchorController>()
        .To(Substitute.For<ILocalPlayerSurfaceAnchorController>());

      controller_ = new MainScreenController(dispatcher_, analytics_, canUseNativeRenderingForDocuments: true);

      timesUpdateScreenCalled_ = 0;
      controller_.ViewTypeChanged += delegate { timesUpdateScreenCalled_++; };
    }

    [TearDown]
    public void Cleanup() {
      ServiceLocator.Clear();
      ServiceLocator.RemoveServiceContainer(SceneManager.GetActiveScene().path);
    }

    [Test]
    public void MainScreenDefaultsToWhiteboard() {
      Assert.AreEqual(MainScreenViewType.WHITEBOARD, controller_.ViewType);

      AssertNoViewTypeChangeEventsDispatched();
    }

    [Test]
    public void MainScreenSwitchesToScreenShareWhenPresenterConnects() {
      dispatcher_.Dispatch(
        new PublicScreenSharerChangedEvent() {
          CallId = 100,
          CurrentSharerId = 1,
        }
      );

      AssertViewTypeChanged(MainScreenViewType.SCREEN_SHARING);
    }

    [Test]
    public void MainScreenSwitchesToGoogleDriveWhenProjectCasted() {
      dispatcher_.Dispatch(new ProjectedDocumentChangedEvent(1));
      AssertViewTypeChanged(MainScreenViewType.GOOGLE_DRIVE);
    }

    [Test]
    public void MainScreenSwitchesStaysOnWhiteboardIfNoPresenterEventIsReceived() {
      dispatcher_.Dispatch(
        new PublicScreenSharerChangedEvent() {
          CallId = 100,
          CurrentSharerId = 0,
        }
      );

      Assert.AreEqual(MainScreenViewType.WHITEBOARD, controller_.ViewType);
      AssertNoViewTypeChangeEventsDispatched();
    }

    [Test]
    public void MainScreenSwitchesBackToWhiteboardIfPresenterWasDisconnected() {
      dispatcher_.Dispatch(
        new PublicScreenSharerChangedEvent() {
          CallId = 100,
          CurrentSharerId = 1,
        }
      );

      AssertViewTypeChanged(MainScreenViewType.SCREEN_SHARING);

      dispatcher_.Dispatch(
        new PublicScreenSharerChangedEvent() {
          CallId = 100,
          CurrentSharerId = 0,
        }
      );

      AssertViewTypeChanged(MainScreenViewType.WHITEBOARD);
    }

    [Test]
    public void MainScreenSwitchesBackToWhiteboardWhenCastStopped() {
      dispatcher_.Dispatch(new ProjectedDocumentChangedEvent(1));
      AssertViewTypeChanged(MainScreenViewType.GOOGLE_DRIVE);

      dispatcher_.Dispatch(new ProjectedDocumentChangedEvent(0));
      AssertViewTypeChanged(MainScreenViewType.WHITEBOARD);
    }

    [Test]
    public void MainScreenViewTypeChangedEventIsOnlyDispatchedForChangesInViewType() {
      dispatcher_.Dispatch(
        new PublicScreenSharerChangedEvent() {
          CallId = 100,
          CurrentSharerId = 1,
        }
      );

      AssertViewTypeChanged(MainScreenViewType.SCREEN_SHARING);

      dispatcher_.Dispatch(
        new PublicScreenSharerChangedEvent() {
          CallId = 100,
          CurrentSharerId = 0,
        }
      );

      AssertViewTypeChanged(MainScreenViewType.WHITEBOARD);

      // emitting the event again, no change to view type should result in no event being dispatched
      dispatcher_.Dispatch(
        new PublicScreenSharerChangedEvent() {
          CallId = 100,
          CurrentSharerId = 0,
        }
      );

      AssertNoViewTypeChangeEventsDispatched();
    }

    private void AssertViewTypeChanged(MainScreenViewType viewType) {
      Assert.AreEqual(viewType, controller_.ViewType);
      AssertDispatcherMainScreenViewTypeChangedEvent(viewType);
      AssertAnalyticsMainScreenViewEvent(viewType);

      Assert.AreEqual(1, timesUpdateScreenCalled_);
      timesUpdateScreenCalled_ = 0;
    }

    private void AssertDispatcherMainScreenViewTypeChangedEvent(MainScreenViewType viewType) {
      dispatcher_.Spy.Received().Dispatch(Arg.Is<MainScreenViewTypeChangedEvent>(e => e.ViewType == viewType));
      dispatcher_.Spy.ClearReceivedCalls();
    }

    private void AssertAnalyticsMainScreenViewEvent(MainScreenViewType viewType) {
      analytics_.Received()
        .Log(
          Arg.Is<MainScreenViewEvent>(
            e => e.GetExtra()[WorkroomsAnalyticsExtraKey.view.ToString()].ToString() == viewType.ToString()
          )
        );
      analytics_.ClearReceivedCalls();
    }

    private void AssertNoViewTypeChangeEventsDispatched() {
      dispatcher_.Spy.DidNotReceive().Dispatch(Arg.Is<MainScreenViewTypeChangedEvent>(e => true));
      dispatcher_.Spy.ClearReceivedCalls();

      analytics_.DidNotReceive().Log(Arg.Is<MainScreenViewEvent>(e => true));
      analytics_.ClearReceivedCalls();

      Assert.AreEqual(0, timesUpdateScreenCalled_);
    }
  }
}
