// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Application;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using CircularDependencyException = Facebook.Workrooms.Application.CircularDependencyException;

namespace Facebook.Workrooms.Tests.Tests.Initialization {
  [OnCall(OnCallName.workrooms_core)]
  public class ServiceInitializationManagerTests {
    private ServiceInitializationTestUtils testUtils_;

    #region Classes for the tests

    private class FooBase {
      public virtual bool IsReady => true;
    }

    private class FooConfig : FooBase, IAppServiceConfig {
      public AppServiceConfig config => new StrictAppServiceConfig<FooBase>(() => new FooConfig());
    }

    private class FooWithDelay : FooBase, IAppServiceConfig {
      public override bool IsReady => isReady_;
      private bool isReady_ = false;

      public AppServiceConfig config =>
        new AppServiceConfig {
          serviceType = typeof(FooBase),
          initializer = async (container) => {
            container.Bind<FooBase>().To(new FooWithDelay());
            await new WaitForSecondsRealtime(5);
            isReady_ = true;
          }
        };
    }

    private class FooWithError : FooBase, IAppServiceConfig {
      public override bool IsReady => false;

      public AppServiceConfig config =>
        new AppServiceConfig {
          serviceType = typeof(FooBase),
          initializer = (container) => {
            throw new ApplicationException("FooWithDelayAndError always fails to be initialized");
          }
        };
    }

    private class Bar1 : IAppServiceConfig {
      public AppServiceConfig config => new StrictAppServiceConfig<Bar1, Bar2>((b) => new Bar1());
    }

    private class Bar2 : IAppServiceConfig {
      public AppServiceConfig config => new StrictAppServiceConfig<Bar2, FooBase>(
        (foo) => {
          if (!foo.IsReady) {
            throw new ApplicationException("We shouldn't have executed if IFoo wasn't ready");
          }

          return new Bar2();
        });
    }

    private class BarWithDelayAndError : IAppServiceConfig {
      public AppServiceConfig config =>
        new AppServiceConfig {
          serviceType = typeof(BarWithDelayAndError),
          initializer = async (container) => {
            container.Bind<BarWithDelayAndError>().To(new BarWithDelayAndError());
            await new WaitForSecondsRealtime(10);
            throw new ApplicationException("BarWithDelayAndError always fails to be initialized");
          }
        };
    }

    private class FooCausingCycle : FooBase, IAppServiceConfig {
      public AppServiceConfig config => new StrictAppServiceConfig<FooBase, Bar1>((b) => new FooCausingCycle());
    }

    private class Baz : IAppServiceConfig {
      public AppServiceConfig config =>
        new AppServiceConfig() {
          serviceType = typeof(Baz),
          dependencies = new[] {typeof(Bar1)},
          initializer = (container) => {
            var foo = container.Get<FooBase>(); // No need to declare IFoo as a dependency, since Bar1 depends on it
            return Task.FromResult(new Baz());
          }
        };
    }

    private class BazWithDelayAndError : IAppServiceConfig {
      public AppServiceConfig config =>
        new AppServiceConfig {
          serviceType = typeof(BazWithDelayAndError),
          initializer = async (container) => {
            container.Bind<BazWithDelayAndError>().To(new BazWithDelayAndError());
            await new WaitForSecondsRealtime(15);
            throw new ApplicationException("BazWithDelayAndError always fails to be initialized");
          }
        };
    }

    // Services from A to G form a single graph with multiple errors
    private class ServiceA : IAppServiceConfig {
      public AppServiceConfig config => new StrictAppServiceConfig<ServiceA>(() => new ServiceA());
    }

    private class ServiceB : IAppServiceConfig {
      public AppServiceConfig config =>
        new StrictAppServiceConfig<ServiceB, ServiceA, ServiceE>((a, e) => new ServiceB());
    }

    private class ServiceC : IAppServiceConfig {
      public AppServiceConfig config => new StrictAppServiceConfig<ServiceC, ServiceB>((b) => new ServiceC());
    }

    private class ServiceD : IAppServiceConfig {
      public AppServiceConfig config =>
        new StrictAppServiceConfig<ServiceD, ServiceC, ServiceG>((c, g) => new ServiceD());
    }

    private class ServiceE : IAppServiceConfig {
      public AppServiceConfig config =>
        new StrictAppServiceConfig<ServiceE, ServiceD, ServiceF>((d, f) => new ServiceE());
    }

    private class ServiceF : IAppServiceConfig {
      public AppServiceConfig config => new StrictAppServiceConfig<ServiceF, ServiceE>((e) => new ServiceF());
    }

    private class ServiceG : IAppServiceConfig {
      public AppServiceConfig config => new StrictAppServiceConfig<ServiceG>(() => new ServiceG());
    }

    #endregion

    [SetUp]
    public void Setup() {
      testUtils_ = new ServiceInitializationTestUtils();
    }

    [TearDown]
    public void Teardown() {
      testUtils_?.Dispose();
    }

    [UnityTest, Timeout(30000)]
    public IEnumerator TestDetectsDuplicatedServicesInLocalList() {
      // Both services bind to IFoo, in the same container
      var task = testUtils_.Run(typeof(FooConfig), typeof(FooWithDelay));
      yield return new WaitUntil(() => task.IsCompleted);

      Assert.IsInstanceOf<DuplicateStartupAction>(
        task.Result.InnerException,
        "Unable to detect that there's 2 services binding the same type"
      );
    }

    [UnityTest, Timeout(30000)]
    public IEnumerator TestDetectsDuplicatedServicesInManager() {
      // Both services bind to IFoo, in the same container
      var task1 = testUtils_.Run(typeof(FooWithDelay));
      var task2 = testUtils_.Run(typeof(FooConfig));
      yield return new WaitUntil(() => task1.IsCompleted && task2.IsCompleted);

      Assert.IsInstanceOf<DuplicateStartupAction>(
        task2.Result.InnerException,
        "Unable to detect that there's 2 services binding the same type"
      );
    }

    [UnityTest, Timeout(30000)]
    public IEnumerator TestAllowsDuplicatedServicesInParentContainer() {
      // Both containers have an IFoo service
      var task1 = testUtils_.Run(testUtils_.ParentContainer, typeof(FooConfig));
      var task2 = testUtils_.Run(testUtils_.ServiceContainer, typeof(FooConfig));
      yield return new WaitUntil(() => task1.IsCompleted && task2.IsCompleted);

      Assert.IsNotInstanceOf<Exception>(
        task2.Result,
        "There shouldn't be any issues with the same service existing in the parent and child containers"
      );
    }

    [UnityTest, Timeout(30000)]
    public IEnumerator TestDetectsMissingDependencies() {
      // Missing dependency IFoo
      var task = testUtils_.Run(typeof(Bar1), typeof(Bar2));
      yield return new WaitUntil(() => task.IsCompleted);

      Assert.IsInstanceOf<MissingDependencyException>(
        task.Result.InnerException,
        "Unable to detect that there's a missing dependency"
      );
    }

    [UnityTest, Timeout(30000)]
    public IEnumerator TestAllowsDependenciesCanBeInitializedInDifferentTasks() {
      // FooWithDelay takes some time to finish, but Bar1 and Bar2 should just wait until that's ready
      var task1 = testUtils_.Run(typeof(FooWithDelay));
      var task2 = testUtils_.Run(typeof(Bar1), typeof(Bar2));
      yield return new WaitUntil(() => task1.IsCompleted && task2.IsCompleted);

      Assert.IsNotInstanceOf<Exception>(
        task1.Result,
        "The task with the dependency was kicked off before, so this shouldn't have caused problems"
      );
    }

    [UnityTest, Timeout(30000)]
    public IEnumerator TestDetectsCircularDependencies() {
      // FooCausingCycle depends on Bar1
      var task = testUtils_.Run(typeof(Bar1), typeof(Bar2), typeof(FooCausingCycle));
      yield return new WaitUntil(() => task.IsCompleted);

      Assert.IsInstanceOf<CircularDependencyException>(task.Result.InnerException, "Unable to detect circular dependency");
    }


    [UnityTest, Timeout(30000)]
    public IEnumerator TestDetectsTransitiveDependencies() {
      // Baz has a transitive dependency in IFoo
      var task = testUtils_.Run(typeof(Baz), typeof(Bar1), typeof(Bar2), typeof(FooConfig));
      yield return new WaitUntil(() => task.IsCompleted);

      Assert.IsNotInstanceOf<Exception>(task.Result, "Unable to detect transitive dependency");
    }

    [UnityTest, Timeout(30000)]
    public IEnumerator TestReportsAllServiceInitializationErrors() {
      // There's 3 errors in this graph: 1 missing dependency (G), and 2 cycles (B > C > D > E > B), (E > F > E)
      var task = testUtils_.Run(
        typeof(ServiceA),
        typeof(ServiceB),
        typeof(ServiceC),
        typeof(ServiceD),
        typeof(ServiceE),
        typeof(ServiceF)
      );
      yield return new WaitUntil(() => task.IsCompleted);

      Assert.IsTrue(
        task.Result.InnerException is AggregateException aggregateException && aggregateException.InnerExceptions.Count == 3,
        "Unable to find all errors"
      );
    }

    [UnityTest, Timeout(30000)]
    public IEnumerator TestImmediatelyReportsInitializationErrors() {
      var task = testUtils_.Run(typeof(BarWithDelayAndError), typeof(BazWithDelayAndError), typeof(FooWithError));

      // FooWithError fails immediately
      yield return new WaitForSecondsRealtime(1);
      Assert.IsTrue(task.IsCompleted, "Task did not finished immediately");
      Assert.IsTrue(
        task.Result.InnerException is ApplicationException,
        "Unable to report the exception from FooWithError"
      );

      // BarWithDelayAndError fails after 10 seconds
      yield return new WaitForSecondsRealtime(11);
      LogAssert.Expect(LogType.Error, new Regex("BarWithDelayAndError always fails to be initialized"));

      // BazWithDelayAndError fails after 15 seconds
      yield return new WaitForSecondsRealtime(16);
      LogAssert.Expect(LogType.Error, new Regex("BazWithDelayAndError always fails to be initialized"));
    }
  }
}
