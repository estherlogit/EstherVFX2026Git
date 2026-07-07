// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using UnityEditor;
using Facebook.Workrooms.Editor;

namespace Facebook.SocialVR.Core.Utils {
  // ASTC decompression occurs at runtime if you're launching in Editor with Android platform selected, which massively
  // slows down time to enter a scene..
  // This popup informs the user on first editor play launch each Unity session, and presents a confirm button
  // to switch platform to windows.
  // Further discussion: https://fb.workplace.com/groups/workrooms.eng.rfcs/permalink/1679759302369660/
  [InitializeOnLoad]
  public static class PlatformCheckPopup {
    public const string MENU_PATH = "Social VR/Tools/";
    private const string TOOLBAR_ENABLE_POPUP_PREF_KEY = "NonWindowsPlatformLaunchPopupEnabled";
    private const string TOOLBAR_ENABLE_MENU_ITEM = MENU_PATH + "Warn about non-windows popup on launch";

    private const string DISABLE_FOR_SESSION_PREF_KEY = "NonWindowsPlatformLaunchPopupSession";

    private static bool showPopup_ = true;

    static PlatformCheckPopup() {
      EditorApplication.playModeStateChanged += OnPlayModeChanged;
      showPopup_ = EditorPrefs.GetBool(TOOLBAR_ENABLE_POPUP_PREF_KEY, true);
    }

    [MenuItem(TOOLBAR_ENABLE_MENU_ITEM)]
    public static void ToggleValidateOnPlay() {
      showPopup_ = !showPopup_;
      EditorPrefs.SetBool(TOOLBAR_ENABLE_POPUP_PREF_KEY, showPopup_);
      Menu.SetChecked(TOOLBAR_ENABLE_MENU_ITEM, showPopup_);
    }

    [MenuItem(TOOLBAR_ENABLE_MENU_ITEM, isValidateFunction: true)]
    public static bool ToggleAutoRegenerationValidate() {
      Menu.SetChecked(TOOLBAR_ENABLE_MENU_ITEM, showPopup_);
      return true;
    }

    private static void OnPlayModeChanged(PlayModeStateChange obj) {
      if (EditorAnalyticsSessionInfo.id.ToString() == EditorPrefs.GetString(DISABLE_FOR_SESSION_PREF_KEY, null)
          || !showPopup_) {
        // Disabled for session or permanently
        return;
      }

      switch (obj) {
        case PlayModeStateChange.EnteredPlayMode:
          if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows) {
            // We don't want to log the time if it was interrupted by this dialog.
            PlayToMeetingLogger.CancelTimer();

            if (EditorUtility.DisplayDialog(
                  "Switch to Windows platform for faster scene load times?",
                  $"Switch from build target platform {EditorUserBuildSettings.activeBuildTarget} to Windows to make runtime scene loads ~30s faster in editor?\n\nFirst switch will take ~5 mins, subsequently ~1 min\n\nDisable this popup via '{TOOLBAR_ENABLE_MENU_ITEM}'",
                  "Switch platform now",
                  "Ignore (and disable for session) "
                )) {
              // Stop playmode, and revert changes when stopped
              EditorApplication.isPlaying = false;
              EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Standalone,
                BuildTarget.StandaloneWindows
              );
            } else {
              // Disable for session
              EditorPrefs.SetString(DISABLE_FOR_SESSION_PREF_KEY, EditorAnalyticsSessionInfo.id.ToString());
            }
          }

          break;
      }
    }
  }
}
