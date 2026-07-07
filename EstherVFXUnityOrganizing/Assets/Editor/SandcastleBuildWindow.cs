// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Castle.Core.Internal;
using Facebook.SocialVR.Tools.Devtools;
using UnityEditor;
using UnityEngine;

namespace Facebook.Workrooms.Editor {
  internal class SandcastleBuildWindow : EditorWindow {
    private string devBuildReason_ = "Awesome testing!";
    private readonly List<string> logLines_ = new List<string>();
    private Vector2 scrollPosition_;
    private string latestSandcastleNonceUrl_;
    private bool buildOngoing_ = false;
    private float progress_ = 0f;
    private string channel_;

    private void OnGUI() {
      scrollPosition_ = EditorGUILayout.BeginScrollView(scrollPosition_, false, false);
      GUILayout.Space(10);
      DevGUI.RenderHelperText(
        "<b>IMPORTANT NOTICE!</b>\nMake sure any changes you want to include in the build are committed locally before starting a dev build",
        color: DevGUI.kDefaultWarnColor,
        style: DevGUI.InfoBoxWrapRichText,
        alignmentStyle: DevGUI.AlignmentStyle.NONE
      );

      GUILayout.Space(20);
      DevGUI.RenderTitle("Sandcastle Dev Build settings");
      devBuildReason_ = EditorGUILayout.TextField("Reason: ", devBuildReason_);
      if (GUILayout.Button("Start dev build")) {
        BuildWorkroomsDevInSandcastle();
        buildOngoing_ = true;
        latestSandcastleNonceUrl_ = string.Empty;
        channel_ = string.Empty;
        progress_ = 0;
      }

      if (buildOngoing_) {
        EditorUtility.DisplayProgressBar("Creating Sandcastle build", "", progress_);
        // dummy bar progress because it looks nice
        if (progress_ < 0.9) {
          progress_ += 0.01f;
        } else {
          progress_ += (1 - progress_) * 0.05f;
        }
      } else {
        EditorUtility.ClearProgressBar();
      }

      GUILayout.Space(20);
      if (!channel_.IsNullOrEmpty()) {
        GUIStyle style = new GUIStyle();
        DevGUI.RenderTitle("Build Results");

        GUILayout.BeginHorizontal(style);
        EditorGUILayout.PrefixLabel("App: ", EditorStyles.boldLabel);
        DevGUI.RenderHelperText(
          $"<size=14><b><color={DevGUI.kDefaultBrightColor}>Workrooms (Dev)</color></b></size>",
          color: DevGUI.kDefaultBrightColor,
          style: DevGUI.InfoTextWrapRichText,
          alignmentStyle: DevGUI.AlignmentStyle.NONE
        );
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(style);
        EditorGUILayout.PrefixLabel($"Store Channel: ", EditorStyles.boldLabel);
        DevGUI.RenderHelperText(
          $"<size=14><b><color={DevGUI.kDefaultBrightColor}>{channel_}</color></b></size>",
          color: DevGUI.kDefaultBrightColor,
          style: DevGUI.InfoTextWrapRichText,
          alignmentStyle: DevGUI.AlignmentStyle.NONE
        );
        GUILayout.EndHorizontal();
      }

      if (!latestSandcastleNonceUrl_.IsNullOrEmpty()) {
        if (GUILayout.Button("See job in sandcastle")) {
          UnityEngine.Application.OpenURL(latestSandcastleNonceUrl_);
        }
      }

      GUILayout.Space(20);

      if (!logLines_.IsNullOrEmpty()) {
        DevGUI.RenderTitle("Latest build setup logs");
        EditorGUILayout.SelectableLabel(
          string.Join("\n", logLines_),
          EditorStyles.textField,
          GUILayout.Height(EditorGUIUtility.singleLineHeight * (logLines_.Count + 1))
        );
      }

      EditorGUILayout.EndScrollView();
    }

    private void BuildWorkroomsDevInSandcastle() {
      var appDataPath = UnityEngine.Application.dataPath;
      var thread = new Thread(() => { SandcastleBuildCommand(appDataPath); });
      thread.Start();
    }

    private void SandcastleBuildCommand(string projectPath) {
      logLines_.Clear();
      latestSandcastleNonceUrl_ = string.Empty;
      var projectRoot = projectPath.Replace("/Assets", "");
      var workroomsCliPath = Path.Combine(projectRoot, "workrooms-cli.bat");

      var processInfo = new ProcessStartInfo {
        FileName = "cmd.exe",
        Arguments =
          $"/c {workroomsCliPath} deploy dev HEAD -r \"{devBuildReason_.Replace("\"", "")}\" --skip-confirmation",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };

      var process = Process.Start(processInfo);
      while (!process.StandardOutput.EndOfStream) {
        string line = process.StandardOutput.ReadLine();
        logLines_.Add(line);
        if (line.StartsWith("Sandcastle Nonce URL:")) {
          latestSandcastleNonceUrl_ = line.Replace("Sandcastle Nonce URL:", "");
        }

        if (line.StartsWith("Preparing a deploy to")) {
          var parts = line.Split(' ');
          channel_ = parts[parts.Length - 1];
        }
      }

      process.WaitForExit();
      process.Close();
      buildOngoing_ = false;
    }

    void OnInspectorUpdate() {
      Repaint();
    }
  }
}
