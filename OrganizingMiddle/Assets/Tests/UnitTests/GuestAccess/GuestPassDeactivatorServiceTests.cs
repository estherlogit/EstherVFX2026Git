// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using Facebook.GraphQL2.Queries;
using Facebook.GraphQLGen.Apps.Workrooms;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.OnCall;
using Facebook.SocialVR.Core.Routes;
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
using UnityEngine.TestTools;

[OnCall(OnCallName.workrooms_adoption)]
public class GuestPassDeactivatorServiceTests {

  private ILog logger_;
  private GraphClientProvider graphClientProvider_;
  private IMainThreadExecutor mainThreadExecutor_;
  private IRouter router_;
  private IDispatcher dispatcher_;
  private IWorkroomsVertsConnector vertsConnector_;
  private IWorkroomsGuestPassService workroomsGuestPassService_;

  private WorkroomsGuestPassProvidesAccessQueryModel hasAccessModel_;

  private const string GuestPassID = "5678";

  [SetUp]
  public void Setup() {
    logger_ = Substitute.For<ILog>();
    ILogService logService = Substitute.For<ILogService>();
    mainThreadExecutor_ = Substitute.For<IMainThreadExecutor>();
    router_ = Substitute.For<IRouter>();
    dispatcher_ = Substitute.For<IDispatcher>();
    vertsConnector_ = Substitute.For<IWorkroomsVertsConnector>();

    hasAccessModel_ = new WorkroomsGuestPassProvidesAccessQueryModel {
      GuestAccessNotification = new WorkroomsGuestPassProvidesAccessQueryModel.GuestAccessNotificationModel() {
        WorkroomsRoom = new WorkroomsGuestPassProvidesAccessQueryModel.GuestAccessNotificationModel.WorkroomsRoomModel() {
          Typename = "WorkroomsRoom"
        }
      }
    };

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

    graphClientProvider_
          .AddMockGraphQLResponse<WorkroomsGuestPassProvidesAccessQuery, WorkroomsGuestPassProvidesAccessQueryModel>(
            hasAccessModel_);

    workroomsGuestPassService_ = new WorkroomsGuestPassService(logger_, graphClientProvider_, mainThreadExecutor_, router_, dispatcher_, vertsConnector_);
  }

  [UnityTest]
  public IEnumerator ConstructionWhenGuestPassIsActiveDeactivatesGuestPass() {
    return ConstructionWhenGuestPassIsActiveDeactivatesGuestPassImpl().AsIEnumerator();
  }

  private async Task ConstructionWhenGuestPassIsActiveDeactivatesGuestPassImpl() {
    await workroomsGuestPassService_.ActivateGuest(GuestPassID);
    Assert.True(workroomsGuestPassService_.GetHasAccessToMeetingRoom());

    var service = new GuestPassDeactivatorService(workroomsGuestPassService_);
    Assert.False(workroomsGuestPassService_.IsGuestActive());
  }
}
