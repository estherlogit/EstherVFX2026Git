// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Facebook.Workrooms.Tests.Tests.Initialization {
  [OnCall(OnCallName.workrooms_core)]
  public class AppServiceConfigTests {
    private ServiceInitializationTestUtils testUtils_;

    private class NotAnAppServiceConfig { }

    private class AppServiceConfigWithoutServiceType : IAppServiceConfig {
      public AppServiceConfig config => new AppServiceConfig {initializer = container => null};
    }

    private class AppServiceConfigWithoutInitializer : IAppServiceConfig {
      public AppServiceConfig config => new AppServiceConfig {serviceType = typeof(NotAnAppServiceConfig)};
    }

    [SetUp]
    public void Setup() {
      testUtils_ = new ServiceInitializationTestUtils();
    }

    [TearDown]
    public void Teardown() {
      testUtils_?.Dispose();
    }

    [UnityTest, Timeout(30000)]
    public IEnumerator TestEnsuresTypesAreAppServiceConfigs() {
      var task = testUtils_.Run(typeof(NotAnAppServiceConfig));
      yield return new WaitUntil(() => task.IsCompleted);

      Assert.IsInstanceOf<ServiceTypeException>(
        task.Result.InnerException,
        "Unable to detect that the class doesn't implement IAppServiceConfig"
      );
    }

    [UnityTest, Timeout(30000)]
    public IEnumerator TestEnsuresAppServiceConfigsHaveServiceTypes() {
      var task = testUtils_.Run(typeof(AppServiceConfigWithoutServiceType));
      yield return new WaitUntil(() => task.IsCompleted);

      Assert.IsInstanceOf<ServiceTypeException>(
        task.Result.InnerException,
        "Unable to detect that the class doesn't have a service type"
      );
    }

    [UnityTest, Timeout(30000)]
    public IEnumerator TestEnsuresAppServicesConfigsHaveInitializerOrCreateBehaviour() {
      var task = testUtils_.Run(typeof(AppServiceConfigWithoutInitializer));
      yield return new WaitUntil(() => task.IsCompleted);

      Assert.IsInstanceOf<StartupActionException>(
        task.Result.InnerException,
        "Unable to detect that the class doesn't have an initializer or a createBehaviour"
      );
    }
  }
}
