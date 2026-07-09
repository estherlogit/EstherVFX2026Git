// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections.Generic;
using Facebook.SocialVR.Core.Bedrock;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.OnCall;
using Facebook.SocialVR.Core.Testutils.Editor;
using Facebook.Workrooms.Analytics.Events;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.Canvas.Service;
using Facebook.Workrooms.NuVerts;
using Facebook.Workrooms.Services;
using Facebook.Workrooms.Testing;
using Facebook.Xplat.Events;
using Facebook.Xplat.Gatekeeper;
using NSubstitute;
using NUnit.Framework;
using Oculus.Verts;
using UnityEngine;

namespace Facebook.Workrooms.Tests.UnitTests {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class CanvasElementInteractionStateServiceTest {

    private CanvasElementInteractionStateService canvasElementInteractionStateService_;
    private CanvasReconciler canvasReconciler_;
    private IDispatcher dispatcher_;
    private WorkroomsNuVertsService workroomsNuVertsService_;
    private NuVertsDriverTestSingle testDriver_;
    private NuVertsDriver nuVertsDriver_;
    private NuVertsComponentContainer<CanvasElementInteractionState> currentInteractionStateContainer_;

    [SetUp]
    public void Setup() {
      dispatcher_ = new DispatcherSpyDecorator(new Dispatcher(), Substitute.For<IDispatcher>());
      Action<CanvasServiceUpdatedEventArgs> onReconcile = (CanvasServiceUpdatedEventArgs e) => { };
      var canvasConfig = Substitute.For<ICanvasConfig>();
      canvasConfig.CanvasDriverMaxUpdatePerFrame.Returns(1);
      canvasReconciler_ = new CanvasReconciler(canvasConfig, onReconcile, Substitute.For<IUpdateRunner>());
      canvasElementInteractionStateService_ = new CanvasElementInteractionStateService(
        Substitute.For<ICanvasDriver>(),
        dispatcher_,
        Substitute.For<ILogService>(),
        canvasReconciler_,
        1
      );
      testDriver_ = new NuVertsDriverTestSingle();
      nuVertsDriver_ = testDriver_.driver;
      workroomsNuVertsService_ = new WorkroomsNuVertsService(
        dispatcher_,
        true,
        new FakeWorkroomsAnalyticsLoggingToggler(),
        new MockGKClient(new Dictionary<string, bool>())
      );
      workroomsNuVertsService_.SetupNuVertsDriver(nuVertsDriver_, isMaster: false);
      workroomsNuVertsService_.OnSceneReady();
    }

    [TearDown]
    public void Cleanup() {
      testDriver_.Dispose();
      workroomsNuVertsService_.Dispose();
      currentInteractionStateContainer_.Dispose();
    }

    [Test]
    public void TestCanvasElementInteractionState() {
      var interactionState = new CanvasElementInteractionState() {
        InteractingPlayerID = 3,
        CanvasEntityID = 1,
        NewPosition = new Vector3(0, 0, 0).ToVertsVec3(),
        NewSize = new Vector3(0, 0, 0).ToVertsVec3(),
        IsAnnotating = Convert.ToByte(false)
      };
      // create element trigger reconcile
      currentInteractionStateContainer_ = workroomsNuVertsService_.CreateComponent(ref interactionState);
      Assert.AreEqual(canvasReconciler_.NumElementsNeedingReconcile, 1);
      canvasReconciler_.ProcessQueue();
      Assert.AreEqual(canvasReconciler_.NumElementsNeedingReconcile, 0);
      // update element trigger reconcile
      currentInteractionStateContainer_.Component.NewPosition = (new Vector3(2, 2, 2)).ToVertsVec3();
      currentInteractionStateContainer_.Component.NewSize = (new Vector3(2, 2, 2)).ToVertsVec3();
      currentInteractionStateContainer_.UpdateComponent();
      Assert.AreEqual(canvasReconciler_.NumElementsNeedingReconcile, 1);
      canvasReconciler_.ProcessQueue();
      Assert.AreEqual(canvasReconciler_.NumElementsNeedingReconcile, 0);
      // delete element trigger reconcile
      currentInteractionStateContainer_.DeleteComponent();
      Assert.AreEqual(canvasReconciler_.NumElementsNeedingReconcile, 1);
    }
  }
}
