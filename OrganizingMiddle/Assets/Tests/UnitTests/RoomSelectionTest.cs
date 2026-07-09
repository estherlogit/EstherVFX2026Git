// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections;
using Facebook.SocialVR.Apps.Workrooms;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.OnCall;
using Facebook.SocialVR.Core.Platform;
using Facebook.Workrooms.Application;
using Facebook.Workrooms.Lobby;
using Facebook.Workrooms.Networking;
using Facebook.Xplat.Gatekeeper;
using Facebook.Xplat.GraphClients;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace Facebook.Workrooms.Tests.UnitTests {
  [OnCall(OnCallName.workrooms_adoption)]
  public class RoomSelectionTest {
    private ILog logger_;
    private IPlatform platform_;
    private IWorkroomsConfigurationOptions workroomsConfigurationOptions_;
    private WorkroomsSettings settings_;
    private GraphClientProvider graphClientProvider_;
    private GatekeeperClient gatekeeperClient_;
    private IWorkroomsDeeplinkingService deeplinkingService_;

    [SetUp]
    public void Setup() {
      logger_ = Substitute.For<ILog>();
      workroomsConfigurationOptions_ = Substitute.For<IWorkroomsConfigurationOptions>();
      settings_ = new WorkroomsSettings();
      gatekeeperClient_ = new GatekeeperClient(null);
      gatekeeperClient_.MockGK(Gatekeepers.GatekeeperToNameMap[GK.WORKROOMS_OCULUS_CALENDAR_DEEPLINK], true);
      deeplinkingService_ = Substitute.For<IWorkroomsDeeplinkingService>();
    }

    private RoomSelection CreateNewRoomSelection() {
      return new RoomSelection(
        logger_,
        workroomsConfigurationOptions_,
        settings_,
        gatekeeperClient_,
        deeplinkingService_,
        true
      );
    }

    [UnityTest]
    public IEnumerator TestPreconfiguredMeetingIdIsPassedFromConfigOptions() {
      workroomsConfigurationOptions_.AutoSelectedWorkroomsRoomID.Returns("4567");
      var roomSelection = CreateNewRoomSelection();
      Assert.AreEqual((roomSelection.GetPreconfiguredRoomId()).Item1, "4567");
      yield return null;
    }

    [UnityTest]
    public IEnumerator TestMeetingIdIsNullIfNotPreconfigured() {
      var roomSelection = CreateNewRoomSelection();
      Assert.IsNull((roomSelection.GetPreconfiguredRoomId()).Item1);
      yield return null;
    }

    [UnityTest]
    public IEnumerator TestMeetingIdFromSettingsIsOnlyPickedUpIfEnabledByConfig() {
      settings_.SelectedWorkroomsRoomId = "6789";
      var roomSelection = CreateNewRoomSelection();
      Assert.IsNull((roomSelection.GetPreconfiguredRoomId()).Item1);
      workroomsConfigurationOptions_.AutoJoinPreselectedWorkroomsRoom.Returns(true);
      var roomSelectionWithConfig = CreateNewRoomSelection();
      Assert.AreEqual((roomSelectionWithConfig.GetPreconfiguredRoomId()).Item1, "6789");
      yield return null;
    }

    [UnityTest]
    public IEnumerator TestConfigOptionsPrecedesSettingsForPreconfiguredMeetingId() {
      settings_.SelectedWorkroomsRoomId = "6789";
      workroomsConfigurationOptions_.AutoSelectedWorkroomsRoomID.Returns("4567");
      workroomsConfigurationOptions_.AutoJoinPreselectedWorkroomsRoom.Returns(true);
      var roomSelection = CreateNewRoomSelection();
      Assert.AreEqual((roomSelection.GetPreconfiguredRoomId()).Item1, "4567");
      yield return null;
    }

    [UnityTest]
    public IEnumerator TestConfigOptionsPrecedesMetaportForPreconfiguredMeetingId() {
      deeplinkingService_.DeeplinkDetails?.WorkroomsRoomID.Returns(1234);
      workroomsConfigurationOptions_.AutoSelectedWorkroomsRoomID.Returns("4567");
      var roomSelection = CreateNewRoomSelection();
      Assert.AreEqual((roomSelection.GetPreconfiguredRoomId()).Item1, "4567");
      yield return null;
    }

    [UnityTest]
    public IEnumerator TestSettingsPrecedesMetaportForPreconfiguredMeetingId() {
      settings_.SelectedWorkroomsRoomId = "6789";
      deeplinkingService_.DeeplinkDetails?.WorkroomsRoomID.Returns(1234);
      workroomsConfigurationOptions_.AutoJoinPreselectedWorkroomsRoom.Returns(true);
      var roomSelection = CreateNewRoomSelection();
      Assert.AreEqual((roomSelection.GetPreconfiguredRoomId()).Item1, "6789");
      yield return null;
    }
  }
}
