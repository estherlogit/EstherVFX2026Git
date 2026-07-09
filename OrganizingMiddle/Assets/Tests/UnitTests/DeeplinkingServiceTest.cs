// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections;
using System.Collections.Generic;
using Facebook.GraphQL2.Mutations;
using Facebook.GraphQLGen.Oculus.Apps.Workrooms;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.OnCall;
using Facebook.SocialVR.Core.Platform;
using Facebook.Workrooms.Application;
using Facebook.Workrooms.Lobby;
using Facebook.Xplat.Analytics;
using Facebook.Xplat.FbLogin;
using Facebook.Xplat.Gatekeeper;
using Facebook.Xplat.GraphClients;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Facebook.Workrooms.Tests.UnitTests {

  [OnCall(OnCallName.workrooms_adoption)]
  public class DeeplinkingServiceTest {
    private ILog logger_;
    private IPlatform platform_;
    private IWorkroomsConfigurationOptions workroomsConfigurationOptions_;
    private OCGraphClientProvider ocGraphClientProvider_;
    private GatekeeperClient gatekeeperClient_;
    private IAnalytics analytics_;

    [SetUp]
    public void Setup() {
      logger_ = Substitute.For<ILog>();
      platform_ = Substitute.For<IPlatform>();
      workroomsConfigurationOptions_ = Substitute.For<IWorkroomsConfigurationOptions>();
      analytics_ = Substitute.For<IAnalytics>();
      ocGraphClientProvider_ = new OCGraphClientProvider();
      var fakeLogin = new FbLogin(
        new FbLogin.Config() {
          appId = "",
          clientToken = "",
          tier = "",
          listeners = new List<FbLogin.IListener>() { },
        }
      );
      ocGraphClientProvider_.CreateMock(fakeLogin);
      ocGraphClientProvider_
        .AddMockGraphQLResponse<OculusWRReportDeeplinkStatusMutation, OculusWRReportDeeplinkStatusMutationModel>(
          new OculusWRReportDeeplinkStatusMutationModel()
        );
    }

    private IWorkroomsDeeplinkingService CreateDeeplinkingService() {
      return new WorkroomsDeeplinkingService(
        logger_,
        platform_,
        ocGraphClientProvider_,
        workroomsConfigurationOptions_,
        analytics_
      );
    }

    [UnityTest]
    public IEnumerator TestWhenInvalidJSONPassedThenDeeplinkIsNull() {
      // handle invalid json
      const string invalidDeepLinkData = "{\"invalid json here\"";
      platform_.GetLaunchDetailsDeeplink().Returns(invalidDeepLinkData);
      var deeplinkingService = CreateDeeplinkingService();
      Assert.IsNull(deeplinkingService.DeeplinkDetails);

      // handle empty deep link data
      const string emptyDeepLinkData = "";
      platform_.GetLaunchDetailsDeeplink().Returns(emptyDeepLinkData);
      deeplinkingService = CreateDeeplinkingService();
      Assert.IsNull(deeplinkingService.DeeplinkDetails);

      // handle deep link without meetingId
      const string dataWithoutMeetingId = "{}";
      platform_.GetLaunchDetailsDeeplink().Returns(dataWithoutMeetingId);
      deeplinkingService = CreateDeeplinkingService();
      Assert.IsNull(deeplinkingService.DeeplinkDetails?.WorkroomsRoomID);
      yield return null;
    }

    [UnityTest]
    public IEnumerator TestWhenJSONPassedThenDeeplinkIsInitialized() {
      const string deeplinkJSON =
        "{\"workrooms_room_id\":12345,\"workrooms_is_remote_wakeup\":true,\"workrooms_is_scheduled\":false,\"last_launch_trace_id\":\"abcdefgh\"}";
      platform_.GetLaunchDetailsDeeplink().Returns(deeplinkJSON);
      var deeplinkingService = CreateDeeplinkingService();
      Assert.IsNotNull(deeplinkingService.DeeplinkDetails);
      Assert.AreEqual(deeplinkingService.DeeplinkDetails?.WorkroomsRoomID, 12345);
      yield return null;
    }
  }
}
