// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Facebook.SocialVR.Core.Adapter;
using Facebook.SocialVR.Core.Bedrock;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Login;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Application;
using NUnit.Framework;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Modules.CodeGenPrefabs;
using Facebook.Workrooms.Application.Initialization;
using Facebook.Workrooms.ContextNavigation;
using Facebook.Workrooms.Networking;
using Facebook.Workrooms.Prefabs.BootstrapReferences;
using Facebook.Workrooms.Scene;
using Facebook.Workrooms.Seating;
using Facebook.Workrooms.Services;
using Facebook.Workrooms.Surfaces;
using Facebook.Workrooms.Utils;
using Facebook.Xplat.Analytics;
using Facebook.Xplat.FbLogin;
using Oculus.Verts;
using UnityEngine;
using UnityEngine.TestTools;

namespace Facebook.Workrooms.Tests.Tests.Initialization {
  [OnCall(OnCallName.workrooms_core)]
  public class AppInitializationTests {
    private class TestCredentialsService : WorkroomsCredentialsServiceWithIdentity {
      public override AppServiceConfig config =>
        (base.config as StrictAppServiceConfigBase)!.InitAfter<WorkroomsSceneInitializer.Starter, NuxRunner>();
    }

    private IServiceContainer appContainer_;
    private IServiceContainer lobbyContainer_;
    private IServiceContainer meetingsContainer_;
    private ServiceInitializationManager manager_;
    private GameObject rootObject_;
    private ComponentContainer componentContainer_;
    private ServiceConfigs serviceConfigs_;
    private WorkroomsConfigurationOptions defaultWorkroomsConfig_;
    private WorkroomsConfigurationOptions vcBridgeWorkroomsConfig_;
    private IRuntimeContextConfig clientRuntimeConfig_;
    private IRuntimeContextConfig masterRuntimeConfig_;
    private IRuntimeContextConfig vcBridgeRuntimeConfig_;
    private ILogService logService_;

    [OneTimeSetUp]
    public void OneTimeSetUp() {
      logService_ = new UnityLogService();
      rootObject_ = new GameObject("AppTests");
      componentContainer_ = new ComponentContainer(rootObject_);
      serviceConfigs_ = CodeGenPrefabUtils.GetAllScriptableObjectsOfType<ServiceConfigs>().First();

      defaultWorkroomsConfig_ = new WorkroomsConfigurationOptions(
        new[] {"--skip-nux", "--create-default-desk", "--create-default-whiteboard", "--enable-desktop-mode"}
      );

      vcBridgeWorkroomsConfig_ = new WorkroomsConfigurationOptions(
        new[] {
          "--skip-nux",
          "--create-default-desk",
          "--create-default-whiteboard",
          "--enable-desktop-mode",
          "--player-configuration-type",
          "VC_BRIDGE"
        }
      );

      clientRuntimeConfig_ = new RuntimeContextConfig(
        defaultWorkroomsConfig_,
        new DefaultBootstrapEnvironmentValues() {
          defaultRunMode = RunMode.CLIENT,
          defaultHasIdentity = true,
          defaultTargetEnvironment = TargetEnvironment.TEST
        },
        componentContainer_
      );

      masterRuntimeConfig_ = new RuntimeContextConfig(
        defaultWorkroomsConfig_,
        new DefaultBootstrapEnvironmentValues() {
          defaultRunMode = RunMode.MASTER,
          defaultHasIdentity = false,
          defaultTargetEnvironment = TargetEnvironment.TEST
        },
        componentContainer_
      );

      vcBridgeRuntimeConfig_ = new RuntimeContextConfig(
        vcBridgeWorkroomsConfig_,
        new DefaultBootstrapEnvironmentValues() {
          defaultRunMode = RunMode.CLIENT,
          defaultHasIdentity = true,
          defaultTargetEnvironment = TargetEnvironment.TEST
        },
        componentContainer_
      );
    }

    [SetUp]
    public void SetUp() {
      var timer = new BootstrapTimer();
      appContainer_ = new ServiceContainer(rootObject_.name);
      lobbyContainer_ = new ServiceContainer("LobbyScene", appContainer_);
      meetingsContainer_ = new ServiceContainer("MeetingScene", appContainer_);
      manager_ = new ServiceInitializationManager(timer);

      appContainer_.Bind<BootstrapTimer>().To(timer);
      appContainer_.Bind<ServiceInitializationManager>().To(manager_);
      appContainer_.Bind<IServicesMonoBehaviourManager>().To(new ServicesMonoBehaviourManager(rootObject_));
      appContainer_.Bind<IDOTweenService>().To(new DOTweenService());
      appContainer_.Bind<VertsDriverPrefabProvider>().To(new VertsDriverPrefabProvider(rootObject_));

      // Register services found in the Unity scene. Note that some of the services are monoBehaviours,
      // but we're binding them with a constructor to avoid executing their Awake();
      meetingsContainer_.Bind<NuVertsDriver>().To(new NuVertsDriver());
      meetingsContainer_.Bind<VertsDriver>().To(new VertsDriver());
      meetingsContainer_.Bind<IDeskSurfaceAnchorProvider>().To(new SeatLayoutView());
    }

    [TearDown]
    public void Teardown() {
      appContainer_.Clear();
      lobbyContainer_.Clear();
      meetingsContainer_.Clear();
    }

    [UnityTest] [Timeout(30000)]
    public IEnumerator CheckDependenciesAreConfiguredCorrectlyOnClientAppLaunch() {
      yield return CheckAppDependenciesAreCorrectlyConfigured(defaultWorkroomsConfig_, clientRuntimeConfig_);
      yield return CheckLobbyDependenciesAreCorrectlyConfigured(defaultWorkroomsConfig_, clientRuntimeConfig_);
      yield return CheckMeetingDependenciesAreCorrectlyConfigured(defaultWorkroomsConfig_, clientRuntimeConfig_);
    }

    [UnityTest] [Timeout(30000)]
    public IEnumerator CheckDependenciesAreConfiguredCorrectlyOnMasterAppLaunch() {
      yield return CheckAppDependenciesAreCorrectlyConfigured(defaultWorkroomsConfig_, masterRuntimeConfig_);
      yield return CheckMeetingDependenciesAreCorrectlyConfigured(defaultWorkroomsConfig_, masterRuntimeConfig_);
    }

    [UnityTest] [Timeout(30000)]
    public IEnumerator CheckDependenciesAreConfiguredCorrectlyOnVCBridgeClientAppLaunch() {
      yield return CheckAppDependenciesAreCorrectlyConfigured(vcBridgeWorkroomsConfig_, vcBridgeRuntimeConfig_);
      yield return CheckMeetingDependenciesAreCorrectlyConfigured(vcBridgeWorkroomsConfig_, vcBridgeRuntimeConfig_);
    }

    [UnityTest] [Timeout(30000)]
    public IEnumerator CheckDependenciesAreConfiguredCorrectlyOnNUX() {
      appContainer_.Bind<IWorkroomsConfigurationOptions>().To(defaultWorkroomsConfig_);
      appContainer_.Bind<CommandLineParser>().To(defaultWorkroomsConfig_);
      appContainer_.Bind<IRuntimeContextConfig>().To(clientRuntimeConfig_);

      // For this test, assume that we have a single container, and make the ICredentials service
      // depend on the NUX. We don't do this normally, to make the app initialize faster.
      var services = WorkroomsApplicationInitializer.AppServices.Concat(WorkroomsSceneInitializer.SceneServices)
        .ToList();

      // Replace the WorkroomsCredentialsServiceWithIdentity (no NUX dependency)
      // for the TestCredentialsService (has NUX dependency)
      var index = services.FindIndex(type => type == typeof(WorkroomsCredentialsServiceWithIdentity));
      services[index] = typeof(TestCredentialsService);

      // Remove some duplicate services
      services.Remove(typeof(WorkroomsUpdateRunnerProvider));

      // Register all the services to the same container
      var config = ServiceInitializationUtils.CreateGraphConfig(defaultWorkroomsConfig_, clientRuntimeConfig_, false, false);
      var log = logService_.GetLog("NUX Services");
      ServiceInitializationUtils.ValidateServiceList(
        ServiceInitializationUtils.CreateActionsList(log, null, config, services.ToArray(), serviceConfigs_.Configs),
        meetingsContainer_,
        manager_
      );

      yield return null;
    }

    private IEnumerator CheckAppDependenciesAreCorrectlyConfigured(
      WorkroomsConfigurationOptions workroomsConfig,
      IRuntimeContextConfig runtimeConfig
    ) {
      appContainer_.Bind<IWorkroomsConfigurationOptions>().To(workroomsConfig);
      appContainer_.Bind<CommandLineParser>().To(workroomsConfig);
      appContainer_.Bind<IRuntimeContextConfig>().To(runtimeConfig);

      Type[] appServices = WorkroomsApplicationInitializer.AppServices;

      var config = ServiceInitializationUtils.CreateGraphConfig(workroomsConfig, runtimeConfig, false, false);

      // Register app-scope services
      manager_.RegisterServices("App services", ServiceScope.App, config, appContainer_, null, appServices, serviceConfigs_.Configs);

      yield return null;
    }

    private IEnumerator CheckLobbyDependenciesAreCorrectlyConfigured(
      WorkroomsConfigurationOptions workroomsConfig,
      IRuntimeContextConfig runtimeConfig
    ) {
      var log = logService_.GetLog("Lobby Services");
      var config = ServiceInitializationUtils.CreateGraphConfig(workroomsConfig, runtimeConfig, false, false);

      // Check Lobby services
      ServiceInitializationUtils.ValidateServiceList(
        ServiceInitializationUtils.CreateActionsList(
          log,
          ServiceScope.Scene,
          config,
          WorkroomsSceneInitializer.SceneServices,
          null // serviceConfigs
        ),
        lobbyContainer_,
        manager_
      );

      yield return null;
    }

    private IEnumerator CheckMeetingDependenciesAreCorrectlyConfigured(
      WorkroomsConfigurationOptions workroomsConfig,
      IRuntimeContextConfig runtimeConfig
    ) {
      var log = logService_.GetLog("Meeting Services");
      var config = ServiceInitializationUtils.CreateGraphConfig(workroomsConfig, runtimeConfig, true, true);

      // Check Meeting services
      ServiceInitializationUtils.ValidateServiceList(
        ServiceInitializationUtils.CreateActionsList(
          log,
          ServiceScope.Scene,
          config,
          WorkroomsSceneInitializer.SceneServices,
          null // serviceConfigs
        ),
        meetingsContainer_,
        manager_
      );

      yield return null;
    }
  }
}
