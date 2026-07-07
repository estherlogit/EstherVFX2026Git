// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections.Generic;
using Facebook.SocialVR.Core.Bedrock;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Editor.Utils {
  class WorkroomsShaderVariantStripper : IPreprocessShaders {
    private readonly ShaderKeyword keywordAvatarDebug_;
    private readonly ShaderKeyword keywordSssModeOld_;
    private readonly ShaderKeyword keywordSssModeNew_;
    private readonly ShaderKeyword keywordSssModeWorkrooms_;
    private readonly bool isProdBuild_;

    public WorkroomsShaderVariantStripper() {
      // Avatar shader stripping will only be effective in prod builds
      isProdBuild_ = CommandLineParser.Parse().prodBuild;

      // Debug only avatar shader keyword
      keywordAvatarDebug_ = new ShaderKeyword("DEBUG_MODE");

      // SSS is intended to be used only in TDF for testing purposes
      keywordSssModeOld_ = new ShaderKeyword("SSS_MODE_OLD_SSS");
      keywordSssModeNew_ = new ShaderKeyword("SSS_MODE_NEW_SSS");
      keywordSssModeWorkrooms_ = new ShaderKeyword("SSS_MODE_WORKROOMS_SSS");
    }

    private bool StripVariant(ShaderCompilerData shaderVariant) {
      return shaderVariant.shaderKeywordSet.IsEnabled(keywordAvatarDebug_)
             || shaderVariant.shaderKeywordSet.IsEnabled(keywordSssModeOld_)
             || shaderVariant.shaderKeywordSet.IsEnabled(keywordSssModeNew_)
             || shaderVariant.shaderKeywordSet.IsEnabled(keywordSssModeWorkrooms_);
    }

    public int callbackOrder => 0;

    public void OnProcessShader(
      Shader shader,
      ShaderSnippetData snippet,
      IList<ShaderCompilerData> shaderCompilerData
    ) {
      if (!isProdBuild_) {
        return;
      }

      int inputShaderVariantCount = shaderCompilerData.Count;
      for (int i = 0; i < shaderCompilerData.Count; ++i) {
        if (StripVariant(shaderCompilerData[i])) {
          shaderCompilerData.RemoveAt(i);
          --i;
        }
      }

      if (shaderCompilerData.Count < inputShaderVariantCount) {
        float percentage = (float)shaderCompilerData.Count / inputShaderVariantCount * 100f;
        Debug.Log(
          $"WR Stripped {shader.name}({snippet.shaderType}) = Kept / Total = {shaderCompilerData.Count} / {inputShaderVariantCount} = {percentage}% of the generated shader variants remain in the player data."
        );
      }
    }
  }
}
