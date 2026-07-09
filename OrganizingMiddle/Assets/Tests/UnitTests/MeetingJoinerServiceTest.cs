// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.


using Assets.Package.Scripts.Services;
using Facebook.GraphQL2.Mutations;
using Facebook.GraphQLGen.Apps.Workrooms;
using Facebook.SocialVR.Core.i18n;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Analytics.Events.Lobby;
using Facebook.Workrooms.Application;
using Facebook.Workrooms.Automation;
using Facebook.Workrooms.ContextNavigation;
using Facebook.Workrooms.core.workrooms_build;
using Facebook.Workrooms.Lobby;
using Facebook.Workrooms.Networking;
using Facebook.Workrooms.Notifications;
using Facebook.Workrooms.Services;
using Facebook.Xplat.Analytics;
using Facebook.Xplat.Events;
using Facebook.Xplat.FbLogin;
using Facebook.Xplat.GraphClients;
using NSubstitute;
using NUnit.Framework;
using ReactVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.TestTools;

[OnCall(OnCallName.workrooms_adoption)]
public class MeetingLoaderServiceTests {

  private IAnalytics analytics_;
  private IWorkroomsAppBuildEnforcement appBuildEnforcmenent_;
  private IDispatcher dispatcher_;
  private GraphClientProvider graphClientProvider_;
  private IWorkroomsGuestPassService guestPassService_;
  private IKillswitchService killswitchService_;
  private ILog logger_;
  private INotificationManager notificationManager_;
  private IWorkroomsConfigurationOptions workroomsConfig_;
  private IWorkroomsHealthChecksService workroomsHealthChecksService_;
  private IWorkroomsVertsConnector vertsConnector_;

  private IMeetingJoinerService meetingJoinerService_;

  private const string WORKROOMS_ROOM_ID = "100000000000";
  private const string MEETING_ID = "10101010101010";
  private const string GUEST_NOTIFICATION_ID = "11001100110011";

  private Notification notification_;

  private string onVertsDisconnectPayload_;

  private string[] onWrongBuildOpenedPayload_;

  private List<string> onRoomLoadedChangedPayloads_;

  private string onMeetingHitKillswitchPayload_;

  private string onMeetingHitOldApplicationPayload_;



  [SetUp]
  public void Setup() {
    analytics_ = Substitute.For<IAnalytics>();
    appBuildEnforcmenent_ = Substitute.For<IWorkroomsAppBuildEnforcement>();
    logger_ = Substitute.For<ILog>();
    graphClientProvider_ = new GraphClientProvider();
    guestPassService_ = Substitute.For<IWorkroomsGuestPassService>();
    killswitchService_ = Substitute.For<IKillswitchService>();
    ILogService logService = Substitute.For<ILogService>();
    notificationManager_ = Substitute.For<INotificationManager>();
    vertsConnector_ = Substitute.For<IWorkroomsVertsConnector>();
    workroomsConfig_ = Substitute.For<IWorkroomsConfigurationOptions>();
    workroomsHealthChecksService_ = Substitute.For<IWorkroomsHealthChecksService>();
    dispatcher_ = new Dispatcher();

    var fakeLogin = new FbLogin(
      new FbLogin.Config() {
        appId = "",
        clientToken = "",
        tier = "",
        listeners = new List<FbLogin.IListener>() { },
      }
    );
    graphClientProvider_.CreateMock(fakeLogin);

    logService.GetLog(Arg.Any<System.Type>()).ReturnsForAnyArgs(logger_);

    var markers = new AutomationMarkers(dispatcher_, logService);

    meetingJoinerService_ = new MeetingJoinerService(analytics_,
      appBuildEnforcmenent_,
      markers,
      dispatcher_,
      graphClientProvider_,
      guestPassService_,
      killswitchService_,
      logService,
      notificationManager_,
      workroomsConfig_,
      workroomsHealthChecksService_,
      vertsConnector_);

    notification_ = new Notification {
      Type = BaseNotificationsModule.NotificationType.LOAD,
      ActionButtonText = FBT.S("Cancel", "Button to cancel the navigation"),
      FinishCondition = NotificationFinishCondition.MANUAL,
      Priority = NotificationPriority.LOAD,
    };

    meetingJoinerService_.OnVertsDisconnect += HandleOnVertsDisconnect;
    onVertsDisconnectPayload_ = string.Empty;

    meetingJoinerService_.OnWrongBuildOpened += HandleOnWrongBuildOpened;
    onWrongBuildOpenedPayload_ = new string[0];

    meetingJoinerService_.OnRoomLoadedChanged += HandleOnRoomLoadedChanged;
    onRoomLoadedChangedPayloads_ = new List<string>();

    meetingJoinerService_.OnMeetingHitKillswitch += HandleOnMeetingHitKillswitch;
    onMeetingHitKillswitchPayload_ = string.Empty;

    meetingJoinerService_.OnMeetingHitOldApplication += HandleOnMeetingHitOldApplication;
    onMeetingHitOldApplicationPayload_ = string.Empty;
  }

  private void HandleOnVertsDisconnect(string value) {
    onVertsDisconnectPayload_ = value;
  }

  private void HandleOnRoomLoadedChanged(string action, string workroomsRoomID) {
    onRoomLoadedChangedPayloads_.Add(action);
    onRoomLoadedChangedPayloads_.Add(workroomsRoomID);
  }

  private void HandleOnWrongBuildOpened(string enforcedAppID, string enforcedApplicationName, string enforcedVersion, string enforcedVersionCode) {
    onWrongBuildOpenedPayload_ = new[] { enforcedAppID, enforcedApplicationName, enforcedVersion, enforcedVersionCode };
  }

  private void HandleOnMeetingHitKillswitch(string workroomsRoomID) {
    onMeetingHitKillswitchPayload_ = workroomsRoomID;
  }

  private void HandleOnMeetingHitOldApplication(string workroomsRoomID) {
    onMeetingHitOldApplicationPayload_ = workroomsRoomID;
  }

  [UnityTest]
  public IEnumerator MeetingJoinerServiceCanJoinRoomDirectlyWhenNoRoadblocksHappen() {
    return MeetingLoaderServiceCanJoinRoomDirectlyWhenNoRoadblocksHappenImpl().AsIEnumerator();
  }

  private async Task MeetingLoaderServiceCanJoinRoomDirectlyWhenNoRoadblocksHappenImpl() {

    await meetingJoinerService_.JoinRoom(WORKROOMS_ROOM_ID, notification_);

    notificationManager_.Received().Show(notification_);
    analytics_.Received().Log(Arg.Any<JoinMeetingClickedAnalyticsEvent>());
    await vertsConnector_.Received().ConnectToWorkroomsRoom(WORKROOMS_ROOM_ID);
    notificationManager_.Received().Finish(notification_);
  }

  [UnityTest]
  public IEnumerator MeetingJoinerServiceStopsJoiningRoomDirectlyGracefullyWhenJoinCancelled() {
    return MeetingJoinerServiceStopsJoiningRoomDirectlyGracefullyWhenJoinCancelledImpl().AsIEnumerator();
  }

  private async Task MeetingJoinerServiceStopsJoiningRoomDirectlyGracefullyWhenJoinCancelledImpl() {

    string message = "Test Saboteur Message";
    vertsConnector_.ConnectToWorkroomsRoom(WORKROOMS_ROOM_ID).Returns(x => throw new NavigationException(
      new DisconnectReasons { WorkroomsVertsDisconnectReason = WorkroomsVertsDisconnectReason.NAVIGATION_CANCELLED },
      message));

    await meetingJoinerService_.JoinRoom(WORKROOMS_ROOM_ID, notification_);

    notificationManager_.Received().Show(notification_);
    analytics_.Received().Log(Arg.Any<JoinMeetingClickedAnalyticsEvent>());
    await vertsConnector_.Received().ConnectToWorkroomsRoom(WORKROOMS_ROOM_ID);
    Assert.That(onVertsDisconnectPayload_, Is.EqualTo(string.Empty));
    Assert.That(onRoomLoadedChangedPayloads_, Is.EqualTo(new List<string> { "LOADING", WORKROOMS_ROOM_ID, "COMPLETED", WORKROOMS_ROOM_ID }));
    notificationManager_.Received().Finish(notification_);
  }

  [UnityTest]
  public IEnumerator MeetingJoinerServiceStopsJoiningRoomDirectlyWithNoAccess() {
    return MeetingJoinerServiceStopsJoiningRoomDirectlyWithNoAccessImpl().AsIEnumerator();
  }

  private async Task MeetingJoinerServiceStopsJoiningRoomDirectlyWithNoAccessImpl() {

    string message = "Test Saboteur Message";
    vertsConnector_.ConnectToWorkroomsRoom(WORKROOMS_ROOM_ID).Returns(x => throw new NavigationException(
      new DisconnectReasons { WorkroomsVertsDisconnectReason = WorkroomsVertsDisconnectReason.NO_ACCESS_ROOM },
      message));

    await meetingJoinerService_.JoinRoom(WORKROOMS_ROOM_ID, notification_);

    notificationManager_.Received().Show(notification_);
    analytics_.Received().Log(Arg.Any<JoinMeetingClickedAnalyticsEvent>());
    await vertsConnector_.Received().ConnectToWorkroomsRoom(WORKROOMS_ROOM_ID);
    Assert.That(onVertsDisconnectPayload_, Is.EqualTo("You don't have access to this Room"));
    Assert.That(onRoomLoadedChangedPayloads_, Is.EqualTo(new List<string> { "LOADING", WORKROOMS_ROOM_ID, "COMPLETED", WORKROOMS_ROOM_ID }));
    notificationManager_.Received().Finish(notification_);
  }

  [UnityTest]
  public IEnumerator MeetingJoinerServiceEmitsOnWrongBuildOpenedWhenJoiningRoomDirectlyFromWrongApp() {
    return MeetingJoinerServiceEmitsOnWrongBuildOpenedJoiningRoomDirectlyFromWrongAppImpl().AsIEnumerator();
  }

  private async Task MeetingJoinerServiceEmitsOnWrongBuildOpenedJoiningRoomDirectlyFromWrongAppImpl() {

    const string appName = "TDF";
    const string appId = AppIds.STABLE_QUEST;
    const string version = "296280165";
    const string versionCode = "2143";

    vertsConnector_.ConnectToWorkroomsRoom(WORKROOMS_ROOM_ID).Returns(x => throw new RoomEnforcementFailedException(
      new DisconnectReasons { WorkroomsVertsDisconnectReason = WorkroomsVertsDisconnectReason.WRONG_APP },
      appName, appId, version, versionCode));

    await meetingJoinerService_.JoinRoom(WORKROOMS_ROOM_ID, notification_);

    notificationManager_.Received().Show(notification_);
    analytics_.Received().Log(Arg.Any<JoinMeetingClickedAnalyticsEvent>());
    await vertsConnector_.Received().ConnectToWorkroomsRoom(WORKROOMS_ROOM_ID);
    // This is not in the same order as the exception constructor - the event and exception have an order mismatch
    Assert.That(onWrongBuildOpenedPayload_, Is.EqualTo(new[] { appId, appName, version, versionCode }));
    Assert.That(onRoomLoadedChangedPayloads_, Is.EqualTo(new List<string> { "LOADING", WORKROOMS_ROOM_ID, "COMPLETED", WORKROOMS_ROOM_ID }));
    notificationManager_.Received().Finish(notification_);

  }

  [UnityTest]
  public IEnumerator MeetingJoinerServiceEmitsRoomLoadedChangedWhenDirectRoomJoiningIsCanceled() {
    return MeetingJoinerServiceEmitsRoomLoadedChangedWhenDirectRoomJoiningIsCanceledlImpl().AsIEnumerator();
  }

  private async Task MeetingJoinerServiceEmitsRoomLoadedChangedWhenDirectRoomJoiningIsCanceledlImpl() {

    CallPrivateMethod(meetingJoinerService_, "StartNavigation", new object[] { WORKROOMS_ROOM_ID });
    await meetingJoinerService_.CancelNavigation(notification_);

    Assert.That(onRoomLoadedChangedPayloads_, Is.EqualTo(new List<string> { "LOADING", WORKROOMS_ROOM_ID, "CANCELLED", WORKROOMS_ROOM_ID
}));
    notificationManager_.Received().Finish(notification_);
  }

  [UnityTest]
  public IEnumerator MeetingJoinerServiceEmitsRoomLoadedChangedWhenDirectRoomJoiningFails() {
    CallPrivateMethod(meetingJoinerService_, "StartNavigation", new object[] { WORKROOMS_ROOM_ID });
    dispatcher_.Dispatch(new WorkroomsRoomJoinFailedEvent(WorkroomsVertsDisconnectReason.ROOM_FULL));

    Assert.That(onRoomLoadedChangedPayloads_, Is.EqualTo(new List<string> { "LOADING", WORKROOMS_ROOM_ID, "FAILED", WORKROOMS_ROOM_ID }));
    notificationManager_.DidNotReceive().Finish(notification_);

    yield return null;
  }

  [UnityTest]
  public IEnumerator MeetingJoinerServiceEmitsMeetingKillswitchWhenFatalMeetingErrorOccuring() {
    return MeetingJoinerServiceEmitsMeetingKillswitchWhenFatalMeetingErrorOccuringImpl().AsIEnumerator();
  }

  public async Task MeetingJoinerServiceEmitsMeetingKillswitchWhenFatalMeetingErrorOccuringImpl() {
    killswitchService_.IsKillswitchEnabled(IKillswitchService.ErrorLevel.FATAL_MEETING_ERROR, out string a1, out string a2).Returns(true);
    await JoinMeetingAsMember();

    Assert.That(onMeetingHitKillswitchPayload_, Is.EqualTo(WORKROOMS_ROOM_ID));
  }

  [UnityTest]
  public IEnumerator MeetingJoinerServiceEmitsMeetingKillswitchWhenMeetingWarningOccuring() {
    return MeetingJoinerServiceEmitsMeetingKillswitchWhenFatalMeetingErrorOccuringImpl().AsIEnumerator();
  }

  public async Task MeetingJoinerServiceEmitsMeetingKillswitchWhenMeetingWarningOccuringImpl() {
    killswitchService_.IsKillswitchEnabled(IKillswitchService.ErrorLevel.MEETING_WARNING, out string a1, out string a2).Returns(true);
    await JoinMeetingAsMember();

    Assert.That(onMeetingHitKillswitchPayload_, Is.EqualTo(WORKROOMS_ROOM_ID));
  }

  [UnityTest]
  public IEnumerator MeetingJoinerServiceEmitsOldApplicationWhenConfigOverrideIsSet() {
    return MeetingJoinerServiceEmitsOldApplicationWhenConfigOverrideIsSetImpl().AsIEnumerator();
  }

  private async Task MeetingJoinerServiceEmitsOldApplicationWhenConfigOverrideIsSetImpl() {
    workroomsConfig_.ShowAppUpdateDialogOverride.Returns(true);

    await JoinMeetingAsMember();

    Assert.That(onMeetingHitOldApplicationPayload_, Is.EqualTo(WORKROOMS_ROOM_ID));
  }

  [UnityTest]
  public IEnumerator MeetingJoinerServiceEmitsOldApplicationWhenHealthcheckDetectsApplicationIsOld() {
    return MeetingJoinerServiceEmitsOldApplicationWhenHealthcheckDetectsApplicationIsOldImpl().AsIEnumerator();
  }

  private async Task MeetingJoinerServiceEmitsOldApplicationWhenHealthcheckDetectsApplicationIsOldImpl() {
    workroomsHealthChecksService_.IsUpdateRequired().Returns(true);

    await JoinMeetingAsMember();

    Assert.That(onMeetingHitOldApplicationPayload_, Is.EqualTo(WORKROOMS_ROOM_ID));
  }

  [UnityTest]
  public IEnumerator MeetingJoinerServiceCanJoinMeetingWhenNoRoadblocksHappen() {
    return MeetingJoinerServiceCanJoinMeetingWhenNoRoadblocksHappenImpl().AsIEnumerator();
  }

  private async Task MeetingJoinerServiceCanJoinMeetingWhenNoRoadblocksHappenImpl() {

    await meetingJoinerService_.JoinRoom(WORKROOMS_ROOM_ID, notification_);

    notificationManager_.Received().Show(notification_);
    analytics_.Received().Log(Arg.Any<JoinMeetingClickedAnalyticsEvent>());
    await vertsConnector_.Received().ConnectToWorkroomsRoom(WORKROOMS_ROOM_ID);
    notificationManager_.Received().Finish(notification_);
  }

  [UnityTest]
  public IEnumerator MeetingJoinerServiceInitiatesGuestSubscriptionThenJoiningWhenJoiningAsGuestWithAccess() {
    return MeetingJoinerServiceInitiatesGuestSubscriptionThenJoinsRoomWhenJoiningAsGuestWithAccessImpl().AsIEnumerator();
  }

  private async Task MeetingJoinerServiceInitiatesGuestSubscriptionThenJoinsRoomWhenJoiningAsGuestWithAccessImpl() {
    await JoinMeetingAsGuest(hasAccessToRoom: true);

    await guestPassService_.Received().ActivateGuest(GUEST_NOTIFICATION_ID);
    await vertsConnector_.Received().ConnectToWorkroomsRoom(WORKROOMS_ROOM_ID, mrRoomId: null, mrRoomJoinedLocally: false);
  }

  [UnityTest]
  public IEnumerator MeetingJoinerServiceInitiatesGuestSubscriptionThenStopsWhenJoiningAsGuestWithoutAccess() {
    return MeetingJoinerServiceInitiatesGuestSubscriptionThenStopsWhenJoiningAsGuestWithoutAccessImpl().AsIEnumerator();
  }

  private async Task MeetingJoinerServiceInitiatesGuestSubscriptionThenStopsWhenJoiningAsGuestWithoutAccessImpl() {
    await JoinMeetingAsGuest(hasAccessToRoom: false);

    await guestPassService_.Received().ActivateGuest(GUEST_NOTIFICATION_ID);
    await vertsConnector_.DidNotReceive().ConnectToWorkroomsRoom(WORKROOMS_ROOM_ID, mrRoomId: null, mrRoomJoinedLocally: false);
  }

  [UnityTest]
  public IEnumerator MeetingJoinerServiceDoesNotInitiateGuestSubscriptionWhenJoiningAsMember() {
    return MeetingJoinerServiceDoesNotInitiateGuestSubscriptionWhenJoiningAsMemberImpl().AsIEnumerator();
  }

  private async Task MeetingJoinerServiceDoesNotInitiateGuestSubscriptionWhenJoiningAsMemberImpl() {
    await JoinMeetingAsMember();

    await guestPassService_.DidNotReceive().ActivateGuest(GUEST_NOTIFICATION_ID);
  }


  [UnityTest]
  public IEnumerator MeetingJoinerServiceCanInstantiateThenJoinLazyWorkroom() {
    return MeetingJoinerServiceCanInstantiateThenJoinLazyWorkroomImpl().AsIEnumerator();
  }

  private async Task MeetingJoinerServiceCanInstantiateThenJoinLazyWorkroomImpl() {
    graphClientProvider_.AddMockGraphQLResponse<WorkroomsMeetingsInstantiateLazyWorkroomMutation,
      WorkroomsMeetingsInstantiateLazyWorkroomMutationModel>(
      new WorkroomsMeetingsInstantiateLazyWorkroomMutationModel {
        CreateWorkroomForCallLink = new WorkroomsMeetingsInstantiateLazyWorkroomMutationModel.CreateWorkroomForCallLinkModel {
          WorkroomsRoom = new WorkroomsMeetingsInstantiateLazyWorkroomMutationModel.CreateWorkroomForCallLinkModel.WorkroomsRoomModel {
            Id = WORKROOMS_ROOM_ID
          }
        }
      });


    await meetingJoinerService_.JoinMeeting(MEETING_ID, string.Empty, notification_, null);
    notificationManager_.Received().Show(notification_);
    analytics_.Received().Log(Arg.Any<JoinMeetingClickedAnalyticsEvent>());
    await vertsConnector_.Received().ConnectToWorkroomsRoom(WORKROOMS_ROOM_ID);
    notificationManager_.Received().Finish(notification_);
    Assert.That(onRoomLoadedChangedPayloads_, Is.EqualTo(new List<string> { "LOADING", WORKROOMS_ROOM_ID, "COMPLETED", WORKROOMS_ROOM_ID }));

  }

  [UnityTest]
  public IEnumerator MeetingJoinerServiceBailsWhenLazyWorkroomFailsToInstantiate() {
    return MeetingJoinerServiceBailsWhenLazyWorkroomFailsToInstantiateImpl().AsIEnumerator();
  }

  private async Task MeetingJoinerServiceBailsWhenLazyWorkroomFailsToInstantiateImpl() {
    graphClientProvider_.AddMockGraphQLResponse<WorkroomsMeetingsInstantiateLazyWorkroomMutation,
      WorkroomsMeetingsInstantiateLazyWorkroomMutationModel>(
      new WorkroomsMeetingsInstantiateLazyWorkroomMutationModel {
        CreateWorkroomForCallLink = null
      });


    await meetingJoinerService_.JoinMeeting(MEETING_ID, string.Empty, notification_, null);
    notificationManager_.Received().Show(notification_);
    analytics_.Received().Log(Arg.Any<JoinMeetingClickedAnalyticsEvent>());
    await vertsConnector_.DidNotReceive().ConnectToWorkroomsRoom(WORKROOMS_ROOM_ID);
    notificationManager_.Received().Finish(notification_);
    Assert.That(onRoomLoadedChangedPayloads_, Is.EqualTo(new List<string>()));

  }

  private async Task JoinMeetingAsMember() {
    await meetingJoinerService_.JoinMeeting(MEETING_ID, WORKROOMS_ROOM_ID, notification_, null);
  }

  private async Task JoinMeetingAsGuest(bool hasAccessToRoom = true) {
    string roomId = hasAccessToRoom ? WORKROOMS_ROOM_ID : string.Empty;
    await meetingJoinerService_.JoinMeeting(MEETING_ID, roomId, notification_, GUEST_NOTIFICATION_ID);
  }

  private void CallPrivateMethod(Object obj, string methodName, object[] parameters) {
    var method = obj.GetType()
      .GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    method?.Invoke(obj, parameters);
  }





}
