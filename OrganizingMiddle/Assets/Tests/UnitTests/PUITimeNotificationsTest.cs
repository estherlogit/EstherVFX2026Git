using System;
using Facebook.SocialVR.Core.i18n.LocalizationManager;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.PUI;
using NSubstitute;
using NUnit.Framework;

namespace Facebook.Workrooms.Tests.UnitTests {
  [OnCall(OnCallName.workrooms_core)]
  public class PUITimeNotificationsTest {

    [SetUp]
    public void Setup() {
      ServiceLocator.Clear();
      var serviceContainer = ServiceLocator.RootContainer;
      serviceContainer.Bind<ILocalizationManager>().To(Substitute.For<ILocalizationManager>());
      serviceContainer.Bind<ILogService>().To(Substitute.For<ILogService>());
    }

    [Test]
    public void OneHourMeetingTest() {
      // GIVEN
      var now = DateTime.Now;
      var startTime = now.AddHours(1);
      var endTime = now.AddHours(2);
      // WHEN
      var notifications = PUITimeNotifications.GenerateMeetingNotifications(startTime, endTime);
      //THEN 30, 15, 10, 5 minute-left and one overruning notification should be generated
      Assert.AreEqual(5, notifications.Count);
    }

    [Test]
    public void ThirtyMinutesMeetingTest() {
      //GIVEN
      var now = DateTime.Now;
      var startTime = now.AddMinutes(30);
      var endTime = now.AddMinutes(30 + 30);
      //WHEN
      var notifications = PUITimeNotifications.GenerateMeetingNotifications(startTime, endTime);
      //THEN 15, 10, 5 minute-left and one overruning notification should be generated
      Assert.AreEqual(4, notifications.Count);
    }

    [Test]
    public void FourtyFiveMinutesMeetingTest() {
      //GIVEN
      var now = DateTime.Now;
      var startTime = now.AddMinutes(30);
      var endTime = now.AddMinutes(30 + 45);
      //WHEN
      var notifications = PUITimeNotifications.GenerateMeetingNotifications(startTime, endTime);
      //THEN 15, 10, 5 minute-left and one overruning notification should be generated
      Assert.AreEqual(4, notifications.Count);
    }

    [Test]
    public void EndingMeetingTest() {
      //GIVEN the meeting is about to end
      var now = DateTime.Now;
      var startTime = now.AddHours(-1);
      var endTime = now;
      //WHEN
      var notifications = PUITimeNotifications.GenerateMeetingNotifications(startTime, endTime);
      //THEN only overruning meeting notification should be generated
      Assert.AreEqual(1, notifications.Count);
    }

    [Test]
    public void OverruningMeetingTest() {
      //GIVEN meeting is overruning for 10 minutes
      var now = DateTime.Now;
      var startTime = now.AddHours(-1);
      var endTime = now.AddMinutes(-10);
      //WHEN
      var notifications = PUITimeNotifications.GenerateMeetingNotifications(startTime, endTime);
      //THEN no notifications should be generated
      Assert.AreEqual(0, notifications.Count);
    }

    [Test]
    public void InThemiddleOfMeetingTest() {
      //GIVEN 30 minutes left in the meeting
      var now = DateTime.Now;
      var startTime = now.AddMinutes(-30);
      var endTime = now.AddMinutes(30);
      //WHEN
      var notifications = PUITimeNotifications.GenerateMeetingNotifications(startTime, endTime);
      //THEN 5, 10, 15 minutes left and overruning meeting notification should be generated
      Assert.AreEqual(4, notifications.Count);
    }

    [Test]
    public void TenMinutesLeftMeetingTest() {
      //GIVEN 10 minutes left in the meeting
      var now = DateTime.Now;
      var startTime = now.AddMinutes(-50);
      var endTime = now.AddMinutes(10);
      //WHEN
      var notifications = PUITimeNotifications.GenerateMeetingNotifications(startTime, endTime);
      //THEN 5 minutes left and overruning meeting notification should be generated
      Assert.AreEqual(2, notifications.Count);
    }
  }
}
