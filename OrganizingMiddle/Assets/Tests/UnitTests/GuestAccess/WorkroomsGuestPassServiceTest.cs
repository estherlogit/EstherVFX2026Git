// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using Facebook.GraphQL2.Queries;
using Facebook.GraphQLGen.Apps.Workrooms;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.OnCall;
using Facebook.SocialVR.Core.Routes;
using Facebook.SocialVR.Core.Scene;
using Facebook.Workrooms.Networking;
using Facebook.Workrooms.Services;
using Facebook.Xplat.Events;
using Facebook.Xplat.FbLogin;
using Facebook.Xplat.GraphClients;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Facebook.Xplat.Threading;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

[OnCall(OnCallName.workrooms_adoption)]
public class WorkroomsGuestPassServiceTest {
  private ILog logger_;
  private GraphClientProvider graphClientProvider_;
  private IMainThreadExecutor mainThreadExecutor_;
  private IRouter router_;
  private IDispatcher dispatcher_;
  private IWorkroomsVertsConnector vertsConnector_;

  private Scene mockWorkroomsRoomScene_;

  private WorkroomsGuestPassProvidesAccessQueryModel hasAccessModel_;
  private GuestAccessNotificationSubscriptionModel hasAccessSubscriptionModel_;
  private GuestAccessNotificationSubscriptionModel noAccessSubscriptionModel_;

  private const string GuestPassID = "1234";
  private const string RoomEnt = "WorkroomsRoom";

  [SetUp]
  public void Setup() {
    logger_ = Substitute.For<ILog>();
    ILogService logService = Substitute.For<ILogService>();
    mainThreadExecutor_ = Substitute.For<IMainThreadExecutor>();
    router_ = Substitute.For<IRouter>();
    dispatcher_ = Substitute.For<IDispatcher>();
    vertsConnector_ = Substitute.For<IWorkroomsVertsConnector>();

    graphClientProvider_ = new GraphClientProvider();
    var fakeLogin = new FbLogin(
      new FbLogin.Config() {
        appId = "",
        clientToken = "",
        tier = "",
        listeners = new List<FbLogin.IListener>() { },
      }
    );
    graphClientProvider_.CreateMock(fakeLogin);
    hasAccessModel_ = new WorkroomsGuestPassProvidesAccessQueryModel {
      GuestAccessNotification = new WorkroomsGuestPassProvidesAccessQueryModel.GuestAccessNotificationModel() {
        WorkroomsRoom =
          new WorkroomsGuestPassProvidesAccessQueryModel.GuestAccessNotificationModel.WorkroomsRoomModel() {
            Typename = RoomEnt
          }
      }
    };
    hasAccessSubscriptionModel_ = new GuestAccessNotificationSubscriptionModel() {
      GuestAccessNotificationSubscribe =
        new GuestAccessNotificationSubscriptionModel.GuestAccessNotificationSubscribeModel() {
          EventType = WorkroomsRoomGuestAccessEventType.CALL_PARTICIPANTS_CHANGED,
          GuestAccessNotification =
            new GuestAccessNotificationSubscriptionModel.GuestAccessNotificationSubscribeModel.
              GuestAccessNotificationModel() {
                WorkroomsRoom =
                  new GuestAccessNotificationSubscriptionModel.GuestAccessNotificationSubscribeModel.
                    GuestAccessNotificationModel.WorkroomsRoomModel() { Typename = RoomEnt }
              }
        }
    };
    noAccessSubscriptionModel_ = new GuestAccessNotificationSubscriptionModel() {
      GuestAccessNotificationSubscribe =
        new GuestAccessNotificationSubscriptionModel.GuestAccessNotificationSubscribeModel() {
          EventType = WorkroomsRoomGuestAccessEventType.CALL_PARTICIPANTS_CHANGED,
          GuestAccessNotification =
            new GuestAccessNotificationSubscriptionModel.GuestAccessNotificationSubscribeModel.
              GuestAccessNotificationModel() { WorkroomsRoom = null }
        }
    };

    mockWorkroomsRoomScene_ = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
    mockWorkroomsRoomScene_.name = Facebook.Workrooms.Constants.MAIN_SCENE_NAME;
  }

  private void CallPrivateMethod(System.Object obj, string methodName, object[] parameters) {
    var method = obj.GetType()
      .GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    method?.Invoke(obj, parameters);
  }

  private WorkroomsGuestPassService CreateGuestPassService() {
    graphClientProvider_
      .AddMockGraphQLResponse<WorkroomsGuestPassProvidesAccessQuery, WorkroomsGuestPassProvidesAccessQueryModel>(
        hasAccessModel_
      );

    return new WorkroomsGuestPassService(
      logger_,
      graphClientProvider_,
      mainThreadExecutor_,
      router_,
      dispatcher_,
      vertsConnector_
    );
  }

  [UnityTest]
  public IEnumerator TestGuestUserEjectedWhenAccessRevokedWhileInMeetingRoom() {
    yield return TestGuestUserEjectedWhenAccessRevokedWhileInMeetingRoomImpl().AsIEnumerator();
  }

  public async Task TestGuestUserEjectedWhenAccessRevokedWhileInMeetingRoomImpl() {
    // arrange
    router_.IsNavigating = false;
    var service = CreateGuestPassService();
    await service.ActivateGuest(GuestPassID);
    Assert.IsTrue(service.GetHasAccessToMeetingRoom());
    SceneManager.SetActiveScene(mockWorkroomsRoomScene_);
    vertsConnector_.ClearReceivedCalls();

    // act
    CallPrivateMethod(service, "OnGuestPassValidityReceived", new[] {noAccessSubscriptionModel_ as object});

    // assert
    Assert.IsFalse(service.GetHasAccessToMeetingRoom());
    await vertsConnector_.Received().CancelCurrentNavigation();
    await vertsConnector_.Received()
      .NavigateToLobby(
        new DisconnectReasons {
          WorkroomsVertsDisconnectReason = WorkroomsVertsDisconnectReason.GUEST_ACCESS_LOST_ACCESS
        },
        true
      );
  }

  [UnityTest]
  public IEnumerator TestGuestUserNotEjectedWhenAccessRetainedWhileInMeetingRoom() {
    yield return TestGuestUserNotEjectedWhenAccessRetainedWhileInMeetingRoomImpl().AsIEnumerator();
  }

  public async Task TestGuestUserNotEjectedWhenAccessRetainedWhileInMeetingRoomImpl() {
    // arrange
    router_.IsNavigating = false;
    var service = CreateGuestPassService();
    await service.ActivateGuest(GuestPassID);
    Assert.IsTrue(service.GetHasAccessToMeetingRoom());
    SceneManager.SetActiveScene(mockWorkroomsRoomScene_);
    vertsConnector_.ClearReceivedCalls();

    // act
    CallPrivateMethod(service, "OnGuestPassValidityReceived", new[] {hasAccessSubscriptionModel_ as object});

    // assert
    Assert.IsTrue(service.GetHasAccessToMeetingRoom());
    await vertsConnector_.DidNotReceiveWithAnyArgs().CancelCurrentNavigation();
    await vertsConnector_.DidNotReceiveWithAnyArgs()
      .NavigateToLobby(
        new DisconnectReasons { WorkroomsVertsDisconnectReason = WorkroomsVertsDisconnectReason.USER_INITIATED }
      );
  }

  [UnityTest]
  public IEnumerator TestGuestUserNotEjectedWhenAccessRevokedWhileMeetingRoomLoading() {
    yield return TestGuestUserNotEjectedWhenAccessRevokedWhileMeetingRoomLoadingImpl().AsIEnumerator();
  }

  public async Task TestGuestUserNotEjectedWhenAccessRevokedWhileMeetingRoomLoadingImpl() {
    // arrange
    router_.IsNavigating = true;
    var service = CreateGuestPassService();
    await service.ActivateGuest(GuestPassID);
    Assert.IsTrue(service.GetHasAccessToMeetingRoom());
    SceneManager.SetActiveScene(mockWorkroomsRoomScene_);
    vertsConnector_.ClearReceivedCalls();

    // act
    CallPrivateMethod(service, "OnGuestPassValidityReceived", new[] {noAccessSubscriptionModel_ as object});

    // assert
    Assert.IsFalse(service.GetHasAccessToMeetingRoom());

    await vertsConnector_.DidNotReceiveWithAnyArgs().CancelCurrentNavigation();
    await vertsConnector_.DidNotReceiveWithAnyArgs()
      .NavigateToLobby(
        new DisconnectReasons { WorkroomsVertsDisconnectReason = WorkroomsVertsDisconnectReason.USER_INITIATED }
      );
  }

  [UnityTest]
  public IEnumerator TestGuestUserEjectedAfterRoomLoadWhenAccessRevokedWhileMeetingRoomLoading() {
    yield return TestGuestUserEjectedAfterRoomLoadWhenAccessRevokedWhileMeetingRoomLoadingImpl().AsIEnumerator();
  }

  public async Task TestGuestUserEjectedAfterRoomLoadWhenAccessRevokedWhileMeetingRoomLoadingImpl() {
    // arrange
    router_.IsNavigating = true;
    var service = CreateGuestPassService();
    await service.ActivateGuest(GuestPassID);
    Assert.IsTrue(service.GetHasAccessToMeetingRoom());
    SceneManager.SetActiveScene(mockWorkroomsRoomScene_);
    CallPrivateMethod(service, "OnGuestPassValidityReceived", new[] {noAccessSubscriptionModel_ as object});
    vertsConnector_.ClearReceivedCalls();

    // act
    service.OnSceneTransitionComplete(new SceneTransitionCompleteEvent());

    // assert
    Assert.IsFalse(service.GetHasAccessToMeetingRoom());
    await vertsConnector_.Received().CancelCurrentNavigation();
    await vertsConnector_.Received()
      .NavigateToLobby(
        new DisconnectReasons {
          WorkroomsVertsDisconnectReason = WorkroomsVertsDisconnectReason.GUEST_ACCESS_LOST_ACCESS
        },
        true
      );
  }
}
