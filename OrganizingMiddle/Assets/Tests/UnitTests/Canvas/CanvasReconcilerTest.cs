// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using Facebook.SocialVR.Core.Bedrock;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.Canvas.Service;
using NSubstitute;
using NUnit.Framework;

namespace Facebook.Workrooms.Tests.UnitTests {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class CanvasReconcilerTest {

    private CanvasReconciler canvasReconciler_;
    private int reconcileCount = 0;
    private ICanvasConfig canvasConfig_;

    [SetUp]
    public void Setup() {
      canvasConfig_ = Substitute.For<ICanvasConfig>();
      reconcileCount = 0;
      Action<CanvasServiceUpdatedEventArgs> onReconcile = (CanvasServiceUpdatedEventArgs e) => { reconcileCount += 1; };
      canvasReconciler_ = new CanvasReconciler(canvasConfig_, onReconcile, Substitute.For<IUpdateRunner>());
    }

    [Test]
    public void TestRequestReconcileElement() {
      canvasReconciler_.RequestReconcileElement(0);
      canvasReconciler_.RequestReconcileElement(1);
      canvasReconciler_.RequestReconcileElement(2);
      canvasReconciler_.RequestReconcileElement(0);
      Assert.AreEqual(3, canvasReconciler_.NumElementsNeedingReconcile);
    }

    [Test]
    public void TestProcessQueue() {
      canvasConfig_.CanvasDriverMaxUpdatePerFrame.Returns(2);
      canvasReconciler_.RequestReconcileElement(0);
      canvasReconciler_.RequestReconcileElement(1);
      canvasReconciler_.RequestReconcileElement(2);
      canvasReconciler_.ProcessQueue();
      Assert.AreEqual(1, canvasReconciler_.NumElementsNeedingReconcile);
      canvasReconciler_.ProcessQueue();
      Assert.AreEqual(0, canvasReconciler_.NumElementsNeedingReconcile);
      Assert.AreEqual(2, reconcileCount);
    }

    [Test]
    public void TestReconcileQueueCompleteAction() {
      canvasConfig_.CanvasDriverMaxUpdatePerFrame.Returns(2);
      var reconcileComplete = false;
      canvasReconciler_.RequestReconcileElement(0);
      canvasReconciler_.RequestReconcileElement(1);
      canvasReconciler_.RequestReconcileElement(2);
      canvasReconciler_.ReconcileQueueComplete += () => { reconcileComplete = true; };
      canvasReconciler_.ProcessQueue();
      Assert.AreEqual(false, reconcileComplete);
      canvasReconciler_.ProcessQueue();
      Assert.AreEqual(true, reconcileComplete);
    }

    [Test]
    public void TestRequestReconcileClearedCanvas() {
      canvasReconciler_.RequestReconcileClearedCanvas();
      canvasReconciler_.ProcessQueue();
      Assert.AreEqual(1, reconcileCount);
    }
  }
}
