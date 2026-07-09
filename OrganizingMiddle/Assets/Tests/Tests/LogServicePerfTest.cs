// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections;
using System.Collections.Generic;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Scene;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Analytics.Events;
using NSubstitute;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Facebook.Workrooms.Tests.Tests {
  [OnCall(OnCallName.workrooms_prodinfra)]
  public class LogServicePerfTest {
    private IServiceContainer container_;
    private ILogService logService_;
    private ILog logger_;

    [SetUp]
    public void Setup() {
      container_ = ServiceLocator.CreateServiceContainer(SceneManager.GetActiveScene().path);
      container_.Bind<ILogService>().To(new UnityLogService());
      logService_ = container_.Get<ILogService>();
      logger_ = logService_.GetLog("LogServiceTest");
    }

    private void LogTestMessage() {
      logger_.Verbose("Test message {0}", 256);
    }

    [UnityTest, Performance]
    public IEnumerator VerboseLogNotSkippedMemoryAllocationTest() {
      Measure.Method(() => { LogTestMessage(); }).GC().Run();
      yield return null;
    }

    [UnityTest, Performance]
    public IEnumerator VerboseLogSkippedMemoryAllocationTest() {
      logService_.minimumLogChannel = LogChannel.INFO;
      Measure.Method(() => { LogTestMessage(); }).GC().Run();
      yield return null;
    }

    [TearDown]
    public void Cleanup() {
      logger_ = null;
      logService_ = null;
      ServiceLocator.Clear();
      container_.Clear();
      ServiceLocator.RemoveServiceContainer(SceneManager.GetActiveScene().path);
    }
  }
}
