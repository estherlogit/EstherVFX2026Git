// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using Facebook.SocialVR.Core.Utils;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.Testing;
using Facebook.Workrooms.Whiteboard;
using Facebook.Xplat.Events;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Facebook.Workrooms.Tests.UnitTests.Whiteboard {

  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class WhiteboardColliderThicknessControllerTest {
    private IServiceContainer container_;
    private WhiteboardReactInputProvider whiteboardReactInputProvider_;
    private BoxCollider collider_;
    private ICanvasInputController canvasInputController_;
    private WhiteboardColliderThicknessController colliderThicknessController_;
    private IToleranceDrawingProvider toleranceDrawingProvider_;
    private DispatcherSpyDecorator dispatcher_;
    private const float COLLIDER_EXPAND_FAC = 2f;

    [SetUp]
    public void Setup() {
      container_ = ServiceLocator.CreateServiceContainer(SceneManager.GetActiveScene().path);

      var logger = Substitute.For<ILogService>();
      container_.Bind<ILogService>().To(logger);

      canvasInputController_ = Substitute.For<ICanvasInputController>();

      GameObject gObj = new GameObject();
      collider_ = gObj.AddComponent<BoxCollider>();

      var canvasConfig = Substitute.For<ICanvasConfig>();
      canvasConfig.ElementDragColliderZExpandFactor.Returns(COLLIDER_EXPAND_FAC);
      dispatcher_ = new DispatcherSpyDecorator(new Dispatcher(), Substitute.For<IDispatcher>());
      container_.Bind<IDispatcher>().To(dispatcher_);

      toleranceDrawingProvider_ = Substitute.For<IToleranceDrawingProvider>();
      toleranceDrawingProvider_.ToleranceDrawing.Returns(1f);

      colliderThicknessController_ = new WhiteboardColliderThicknessController(
        canvasInputController: canvasInputController_,
        canvasConfig: canvasConfig,
        boxCollider: collider_,
        dispatcher: dispatcher_,
        toleranceDrawingProvider: toleranceDrawingProvider_
      );
    }

    [TearDown]
    public void TearDown() {
      ServiceLocator.Clear();
      container_.Clear();
      Object.DestroyImmediate(collider_.gameObject);
      ServiceLocator.RemoveServiceContainer(SceneManager.GetActiveScene().path);
    }

    [Test]
    public void TestColliderExpandsWhenDraggingCanvasElements() {
      var startSize = collider_.size;

      // Assert that collider_.size.z expands
      canvasInputController_.IsInputActive.Returns(true);
      canvasInputController_.InputStateChanged += Raise.Event<Action>();
      Assert.True(MathUtil.Approximately(collider_.size.z, startSize.z * COLLIDER_EXPAND_FAC));

      canvasInputController_.IsInputActive.Returns(false);
      canvasInputController_.InputStateChanged += Raise.Event<Action>();
      Assert.True(MathUtil.Approximately(collider_.size.z, startSize.z));
    }

    [Test]
    public void TestColliderExpandsWhenDrawingToleranceChanges() {
      var startTolerance = 0.1f;
      toleranceDrawingProvider_.ToleranceDrawing.Returns(startTolerance);
      dispatcher_.Dispatch(new WhiteboardToleranceDrawingChangedEvent());

      var startSize = collider_.size;

      var newTolerance = 0.2f;
      toleranceDrawingProvider_.ToleranceDrawing.Returns(newTolerance);
      dispatcher_.Dispatch(new WhiteboardToleranceDrawingChangedEvent());

      Assert.True(MathUtil.Approximately(collider_.size.z, startSize.z * newTolerance / startTolerance));
    }
    [Test]
    public void ThicknessChangeEventOnlyDispatchesWhenThicknessChanged() {
      dispatcher_.Dispatch(new WhiteboardToleranceDrawingChangedEvent());
      dispatcher_.Spy.ClearReceivedCalls();
      dispatcher_.Dispatch(new WhiteboardToleranceDrawingChangedEvent());
      dispatcher_.Spy.DidNotReceive()
        .Dispatch(Arg.Is<WhiteboardColliderThicknessChangedEvent>(e => true));
    }
  }
}
