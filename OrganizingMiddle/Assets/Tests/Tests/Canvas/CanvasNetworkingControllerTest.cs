// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Facebook.GraphQLGen.Apps.Workrooms;
using Facebook.SocialVR.Core.Adapter;
using Facebook.SocialVR.Core.Analytics;
using Facebook.SocialVR.Core.Bedrock;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Analytics.Events;
using Facebook.Workrooms.Application;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.Services;
using Facebook.Workrooms.Testing;
using Facebook.Xplat.Events;
using Facebook.Xplat.FBGraphQL;
using Facebook.Xplat.Gatekeeper;
using Facebook.Xplat.GraphClients;
using NSubstitute;
using NUnit.Framework;
using Oculus.Verts;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assert = UnityEngine.Assertions.Assert;

namespace Facebook.Workrooms.Tests.Tests.Canvas {
  [Ignore("NuVerts native driver issues are causing editor crashes or sandcastle test failures")]
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class CanvasNetworkingControllerTest {
    private const uint CANVAS_ID = 10000;

    private const int REMOTE_VERTS_SESSION_ID = 3;

    private GameObject testRoot_;
    private CanvasNetworkingControllerIfHasLocalPlayer controller_;
    private NuVertsDriver nuVertsDriver_;
    private ulong nextID_ = 1000;
    private IServiceContainer container_;
    private DispatcherSpyDecorator dispatcher_;
    private GatekeeperClient gkClient_;
    private CanvasPersistence canvasPersistence_;
    private bool hasTriggeredCanvasReconcile_;

    private class TestRuntimeContextConfig : IRuntimeContextConfig {
      public RunMode runMode => RunMode.CLIENT;
      public TargetEnvironment targetEnvironment => TargetEnvironment.TEST;
      public LocalPlayerMode localPlayerMode => LocalPlayerMode.HAS_LOCAL_PLAYER;
      public ComponentContainer gameObject => null;
    }

    [OnCall(OnCallName.workrooms_creative_collaborations)]
    private class MockWorkroomsRoomFragment : WorkroomsRoomFragment {

      public MockWorkroomsRoomFragment(FBGraphQLClient graphQLClient, IDispatcher dispatcher) : base(
        graphQLClient,
        dispatcher
      ) { }

      public override WorkroomsRoomNodeQueryModel.FetchWorkroomsRoomModel Get() {
        return new WorkroomsRoomNodeQueryModel.FetchWorkroomsRoomModel() {
          Id = "5943",
        };
      }
    }

    [SetUp]
    public void Setup() {
      testRoot_ = new GameObject();

      // bind to the app-service container as GetLog() reads from it directly
      var unityLogService = new UnityLogService();
      ServiceLocator.RootContainer.Bind<ILogService>().To(unityLogService);
      ServiceLocator.RootContainer.Bind<RuntimeContext>().To(new RuntimeContext(new TestRuntimeContextConfig()));
      ServiceLocator.RootContainer.Bind<IDevicePlatform>().To(testRoot_.AddComponent<PlatformService>());

      container_ = ServiceLocator.CreateServiceContainer(SceneManager.GetActiveScene().path);
      dispatcher_ = new DispatcherSpyDecorator(new Dispatcher(), Substitute.For<IDispatcher>());
      container_.Bind<IDispatcher>().To(dispatcher_);
      container_.Bind<RuntimeContext>().To(new RuntimeContext(new TestRuntimeContextConfig()));
      var graphClientProvider = new GraphClientProvider();
      container_.Bind<WorkroomsRoomFragment>().To(new MockWorkroomsRoomFragment(graphClientProvider.GraphQLClient, dispatcher_));
      var playerConfiguration = ScriptableObject.CreateInstance<PlayerConfiguration.PlayerConfiguration>();
      container_.Bind<PlayerConfiguration.PlayerConfiguration>().To(playerConfiguration);
      container_.Bind<GraphClientProvider>().To(graphClientProvider);
      gkClient_ = new GatekeeperClient(new FBGraphQLGatekeeperDataLoader(graphClientProvider));
      container_.Bind<GatekeeperClient>().To(gkClient_);
      var sitevarClient = new WorkroomsSitevarClient(null);
      container_.Bind<WorkroomsPlayerManager>()
        .To(
          new WorkroomsPlayerManager(
            dispatcher_,
            unityLogService,
            playerConfiguration,
            0,
            sitevarClient,
            Substitute.For<IQPLLogger>()
          )
        );

      nuVertsDriver_ = new NuVertsDriver();
      nuVertsDriver_.ConnectAsLocalTest(NuVertsDriverConfiguration.Default());
      container_.Bind<NuVertsDriver>().To(nuVertsDriver_);

      var workroomsNuVertsService = new WorkroomsNuVertsService(
        dispatcher_,
        true,
        new FakeWorkroomsAnalyticsLoggingToggler(),
        gkClient_
      );
      workroomsNuVertsService.SetupNuVertsDriver(nuVertsDriver_, isMaster: true);
      container_.Bind<WorkroomsNuVertsService>().To(workroomsNuVertsService);

      controller_ = testRoot_.AddComponent<CanvasNetworkingControllerIfHasLocalPlayer>();

      hasTriggeredCanvasReconcile_ = false;

      canvasPersistence_ = new CanvasPersistence(
        null,
        null,
        null,
        null,
        null
      );
    }

    [TearDown]
    public void Cleanup() {
      nuVertsDriver_.Shutdown();
      NuVertsDriver.StaticDestroy();
      GameObject.DestroyImmediate(testRoot_);
      ServiceLocator.Clear();
      container_.Clear();
      ServiceLocator.RemoveServiceContainer(SceneManager.GetActiveScene().path);
    }

    private ulong GenerateID() {
      return nextID_++;
    }

    private async Task LoadCanvasState(
      uint snapshotID,
      List<CanvasDelayedAction> deltaActions,
      int sessionIncrementalVersion = 0
    ) {
      await controller_.LoadCanvasState(
        CANVAS_ID,
        snapshotID,
        canvasPersistence_.SerializeDelayedActions(deltaActions),
        sessionIncrementalVersion
      );
    }

    private IEnumerator AssertNeedsReconcile() {
      yield return new WaitForUpdate();
      Assert.IsTrue(hasTriggeredCanvasReconcile_, "expected the canvas to need a reconcile but it didn't");
      hasTriggeredCanvasReconcile_ = false;
    }

    private IEnumerator AssertDoesNotNeedReconcile() {
      yield return new WaitForUpdate();
      Assert.IsFalse(hasTriggeredCanvasReconcile_, "expected the canvas to not need a reconcile but it did");
    }
  }
}
