// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Facebook.SocialVR.Core.Services;
using Facebook.Workrooms.Application;
using NUnit.Framework;

namespace Facebook.Workrooms.Tests.Tests.Initialization {
  public class ServiceInitializationTestUtils : IDisposable {
    public IServiceContainer ServiceContainer { get; }
    public IServiceContainer ParentContainer { get; }

    private readonly ServiceInitializationManager serviceInitializationManager_;
    private readonly CancellationTokenSource cancellationTokenSource_;
    private readonly ServiceGraphConfig config_;

    public ServiceInitializationTestUtils(List<ServiceGraphProperty> graphProperties = null) {
      serviceInitializationManager_ = new ServiceInitializationManager();
      ParentContainer = new ServiceContainer("ParentContainer");
      ServiceContainer = new ServiceContainer("TestContainer", ParentContainer);
      cancellationTokenSource_ = new CancellationTokenSource();
      config_ = new ServiceGraphConfig();

      if (graphProperties != null) {
        foreach (var property in graphProperties) {
          config_.Set(property);
        }
      }
    }

    public void Dispose() {
      cancellationTokenSource_?.Cancel();
      ServiceContainer.Clear();
      ParentContainer.Clear();
    }

    public Task<Exception> Run(params Type[] serviceTypes) {
      return Run(ServiceContainer, serviceTypes);
    }

    public async Task<Exception> Run(IServiceContainer serviceContainer, params Type[] serviceTypes) {
      Exception exception = null;
      try {
        await serviceInitializationManager_.Run(
          "Test Services",
          serviceContainer,
          ServiceScope.App,
          config_,
          cancellationTokenSource_,
          serviceTypes,
          null // serviceConfigs
        );
      } catch (Exception ex) {
        exception = ex;
      }

      return exception;
    }
  }
}
