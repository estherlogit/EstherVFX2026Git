// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Tools.Devtools;
using Facebook.Workrooms.Application;
using UnityEditor;
using UnityEngine;

namespace Facebook.Workrooms.Editor {
  [InitializeOnLoad]
  public static class PlayToMeetingLogger {

    private static string transitionToPlayModeStartedTimeStampKey_ = "workrooms_transition_to_play_mode_start";
    private static readonly ILog log_ = new EditorLog();
    private static bool isJoiningPreselectedRoom_;

    static PlayToMeetingLogger() {
      EditorApplication.playModeStateChanged -= PlaymodeStateChanged;
      EditorApplication.playModeStateChanged += PlaymodeStateChanged;
      isJoiningPreselectedRoom_ =
        EditorPrefs.GetBool(WorkroomsConfigurationOptions.AutoJoinPreselectedWorkroomsRoomEditorPref);
    }

    public static void WorkroomsMeetingLocalPlayerReady() {
      var ticks = long.Parse(EditorPrefs.GetString(transitionToPlayModeStartedTimeStampKey_));
      if (ticks == -1) {
        log_.Info($"[PlayToMeetingLogger] Timer was cancelled. Not logging.");
        return;
      }

      var transitionToPlayModeTimeStamp = new DateTime(ticks);
      log_.Info(
        $"[PlayToMeetingLogger] WorkroomsMeetingLocalPlayerReady elapsed time {(DateTime.Now - transitionToPlayModeTimeStamp).TotalSeconds}"
      );
      Dev.Analytics.Value.Log(
        new PlayToMeetingAnalyticsEvent(
          DateTime.Now - transitionToPlayModeTimeStamp,
          EditorUserBuildSettings.activeBuildTarget,
          isJoiningPreselectedRoom_
        )
      );
    }

    public static void CancelTimer() {
      log_.Info($"[PlayToMeetingLogger] Cancelling timer");
      EditorPrefs.SetString(transitionToPlayModeStartedTimeStampKey_, "-1");
    }

    private static void PlaymodeStateChanged(PlayModeStateChange stateChange) {
      log_.Info($"[PlayToMeetingLogger] PlaymodeStateChanged: {stateChange}");
      if (stateChange == PlayModeStateChange.ExitingEditMode) {
        // Using EditorPrefs so this survives across this class re-instantiation after scrips reload before play mode.  
        EditorPrefs.SetString(transitionToPlayModeStartedTimeStampKey_, DateTime.Now.Ticks.ToString());
      }
    }

    private class PlayToMeetingAnalyticsEvent : SocialVRUnityEditorAnalyticsEvent {
      public PlayToMeetingAnalyticsEvent(TimeSpan duration, BuildTarget target, bool isJoiningPreselectedRoom) {
        AddExtraFields("time_spent", duration.TotalSeconds);
        AddExtraFields("build_target", target.ToString());
        AddExtraFields("is_auto_join_selected", isJoiningPreselectedRoom);
      }

      protected override string GetEventSubtype() {
        return "PlayToMeeting";
      }
    }
  }
}
