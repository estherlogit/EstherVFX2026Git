// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections.Generic;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Services;
using Facebook.Workrooms.TouchInputInteractor;
using Facebook.Workrooms.InputDevice;
using Facebook.Workrooms.Testing;
using Facebook.Xplat.Events;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using Facebook.SocialVR.Core.OnCall;

namespace Facebook.Workrooms.Tests.UnitTests {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class TouchInputInteractorTest : MonoBehaviour {

    private DispatcherSpyDecorator dispatcher_;
    private IServiceContainer container_;
    private TouchInputInteractor.TouchInputInteractor touchInputInteractor_;

    private BoxCollider collisionTargetPriorityMinus1_;
    private BoxCollider collisionTargetPriority0_;
    private BoxCollider collisionTargetPriority1_;

    private readonly Dictionary<Collider, TouchInputInteractorTarget> registeredCollisionTargets_ =
      new Dictionary<Collider, TouchInputInteractorTarget>();

    [SetUp]
    public void Setup() {
      container_ = ServiceLocator.CreateServiceContainer(SceneManager.GetActiveScene().path);
      dispatcher_ = new DispatcherSpyDecorator(new Dispatcher(), Substitute.For<IDispatcher>());
      container_.Bind<IDispatcher>().To(dispatcher_);

      var inputMethodService = Substitute.For<IInputMethodService>();
      container_.Bind<IInputMethodService>().To(inputMethodService);

      inputMethodService.GetTouchInputInteractorTargets().Returns(registeredCollisionTargets_);

      var logger = Substitute.For<ILogService>();
      container_.Bind<ILogService>().To(logger);
    }

    private BoxCollider CreateCollisionTarget(int priority) {
      var collider = (new GameObject("priority " + priority)).AddComponent<BoxCollider>();
      collider.gameObject.AddComponent<TouchInputInteractorTarget>().Priority = priority;
      return collider;
    }

    private void CallPrivateMethod(System.Object obj, string methodName, object[] parameters) {
      var method = obj.GetType()
        .GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      method?.Invoke(obj, parameters);
    }

    private void SetPrivateField(Object obj, string fieldName, object fieldValue) {
      var property = obj.GetType()
        .GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      property?.SetValue(obj, fieldValue);
    }

    [TearDown]
    public void Cleanup() {
      ServiceLocator.Clear();
      container_.Clear();
      ServiceLocator.RemoveServiceContainer(SceneManager.GetActiveScene().path);
    }

    [Test]
    public void TriviallyColliding() {
      RecreateComponents(true);
      MoveIntoCollisionRange(collisionTargetPriority0_);
      touchInputInteractor_.LateUpdate();
      Assert.IsTrue(touchInputInteractor_.CurrentHitCollider == collisionTargetPriority0_);
    }

    [Test]
    public void TriviallyNotColliding() {
      RecreateComponents(true);
      touchInputInteractor_.LateUpdate();
      Assert.IsNull(touchInputInteractor_.CurrentHitCollider);
    }

    [Test]
    public void WhenCollisionsActiveThenTouchInputInteractorChangeTargetEventDispatched() {
      RecreateComponents();
      TriggerEnter(collisionTargetPriority0_);
      touchInputInteractor_.LateUpdate();
      dispatcher_.Spy.Received().Dispatch(Arg.Is<TouchInputInteractorChangeTargetEvent>(e => true));
    }

    [Test]
    public void WhenTargetDisabledThenActiveCollisionStops() {
      RecreateComponents();

      TriggerEnter(collisionTargetPriority0_);
      touchInputInteractor_.LateUpdate();
      dispatcher_.Spy.Received()
        .Dispatch(Arg.Is<TouchInputInteractorChangeTargetEvent>(e => e.Target == collisionTargetPriority0_.gameObject));
      dispatcher_.Spy.ClearReceivedCalls();

      collisionTargetPriority0_.gameObject.SetActive(false);
      touchInputInteractor_.LateUpdate();
      dispatcher_.Spy.Received().Dispatch(Arg.Is<TouchInputInteractorChangeTargetEvent>(e => e.Target == null));
      dispatcher_.Spy.ClearReceivedCalls();
    }

    [Test]
    public void WhenTargetDestroyedThenActiveCollisionStops() {
      RecreateComponents();

      TriggerEnter(collisionTargetPriority0_);
      touchInputInteractor_.LateUpdate();
      dispatcher_.Spy.Received()
        .Dispatch(Arg.Is<TouchInputInteractorChangeTargetEvent>(e => e.Target == collisionTargetPriority0_.gameObject));
      dispatcher_.Spy.ClearReceivedCalls();

      DestroyImmediate(collisionTargetPriority0_);
      touchInputInteractor_.LateUpdate();
      dispatcher_.Spy.Received().Dispatch(Arg.Is<TouchInputInteractorChangeTargetEvent>(e => e.Target == null));
      dispatcher_.Spy.ClearReceivedCalls();
    }

    [Test]
    public void WhenCollisionsDisabledForTargetThenTargetIsNotHitUntilCollisionsReenabled() {
      RecreateComponents();

      var inputTarget0 = collisionTargetPriority0_.GetComponent<ITouchInputInteractorTarget>();
      inputTarget0.TargetType = TouchInputInteractorTarget.Type.CANVAS_ELEMENT;
      touchInputInteractor_.DisableInteractionForType(TouchInputInteractorTarget.Type.CANVAS_ELEMENT);
      TriggerEnter(collisionTargetPriority0_);
      touchInputInteractor_.LateUpdate();
      dispatcher_.Spy.DidNotReceive()
        .Dispatch(Arg.Is<TouchInputInteractorChangeTargetEvent>(e => e.Target == collisionTargetPriority0_.gameObject));
      dispatcher_.Spy.ClearReceivedCalls();

      touchInputInteractor_.EnableInteractionForType(TouchInputInteractorTarget.Type.CANVAS_ELEMENT);
      touchInputInteractor_.LateUpdate();
      dispatcher_.Spy.Received()
        .Dispatch(Arg.Is<TouchInputInteractorChangeTargetEvent>(e => e.Target == collisionTargetPriority0_.gameObject));
      dispatcher_.Spy.ClearReceivedCalls();
    }

    [Test]
    public void WhenMultipleCollisionsActiveThenHighestPriorityCollisionIsHit() {
      RecreateComponents();

      // Highest priority is hit
      TriggerEnter(collisionTargetPriority0_);
      TriggerEnter(collisionTargetPriority1_);
      touchInputInteractor_.LateUpdate();
      dispatcher_.Spy.Received()
        .Dispatch(Arg.Is<TouchInputInteractorChangeTargetEvent>(e => e.Target == collisionTargetPriority1_.gameObject));
      dispatcher_.Spy.ClearReceivedCalls();

      // Highest priority collider removed, now lower priority collider should be hit
      TriggerExit(collisionTargetPriority1_);
      touchInputInteractor_.LateUpdate();
      dispatcher_.Spy.Received()
        .Dispatch(Arg.Is<TouchInputInteractorChangeTargetEvent>(e => e.Target == collisionTargetPriority0_.gameObject));
      dispatcher_.Spy.ClearReceivedCalls();
    }

    [Test]
    public void WhenCollisionAlreadyActiveThenNewCollisionDoesNotInterrupt() {
      RecreateComponents();

      // Lower priority active
      TriggerEnter(collisionTargetPriority0_);
      touchInputInteractor_.LateUpdate();
      dispatcher_.Spy.Received()
        .Dispatch(Arg.Is<TouchInputInteractorChangeTargetEvent>(e => e.Target == collisionTargetPriority0_.gameObject));
      dispatcher_.Spy.ClearReceivedCalls();

      // Higher priority collision doesn't interrupt
      TriggerEnter(collisionTargetPriority1_);
      touchInputInteractor_.LateUpdate();
      dispatcher_.Spy.DidNotReceive()
        .Dispatch(Arg.Is<TouchInputInteractorChangeTargetEvent>(e => e.Target == collisionTargetPriority1_.gameObject));
      dispatcher_.Spy.ClearReceivedCalls();

      // New collision starts when active collision exits
      TriggerExit(collisionTargetPriority0_);
      touchInputInteractor_.LateUpdate();
      dispatcher_.Spy.Received()
        .Dispatch(Arg.Is<TouchInputInteractorChangeTargetEvent>(e => e.Target == collisionTargetPriority1_.gameObject));
      dispatcher_.Spy.ClearReceivedCalls();
    }

    [Test]
    public void WhenInputColliderDisabledThenActiveHitsAreCleared() {
      RecreateComponents();

      TriggerEnter(collisionTargetPriority0_);
      touchInputInteractor_.LateUpdate();
      dispatcher_.Spy.Received()
        .Dispatch(Arg.Is<TouchInputInteractorChangeTargetEvent>(e => e.Target == collisionTargetPriority0_.gameObject));
      dispatcher_.Spy.ClearReceivedCalls();

      // Simulate input collider being disabled
      CallPrivateMethod(touchInputInteractor_, "OnDisable", new object[] { });
      dispatcher_.Spy.Received().Dispatch(Arg.Is<TouchInputInteractorChangeTargetEvent>(e => e.Target == null));
      dispatcher_.Spy.ClearReceivedCalls();

      // We need to explicitly move the target out of collision range, or the new code path will instantly
      // detect the collision on the next call to LateUpdate(), triggering an event dispatch.
      MoveOutOfCollisionRange(collisionTargetPriority0_);

      // Simulate input collider being re-enabled
      touchInputInteractor_.LateUpdate();
      dispatcher_.Spy.DidNotReceive()
        .Dispatch(Arg.Is<TouchInputInteractorChangeTargetEvent>(e => e.Target == collisionTargetPriority0_.gameObject));
      dispatcher_.Spy.ClearReceivedCalls();
    }

    [Test]
    public void WhenTargetColliderDisabledThenCollisionEnds() {
      RecreateComponents();

      TriggerEnter(collisionTargetPriority0_);
      touchInputInteractor_.LateUpdate();
      dispatcher_.Spy.Received()
        .Dispatch(Arg.Is<TouchInputInteractorChangeTargetEvent>(e => e.Target == collisionTargetPriority0_.gameObject));
      dispatcher_.Spy.ClearReceivedCalls();

      // Simulate input collider being disabled
      collisionTargetPriority0_.enabled = false;
      touchInputInteractor_.LateUpdate();
      dispatcher_.Spy.Received().Dispatch(Arg.Is<TouchInputInteractorChangeTargetEvent>(e => e.Target == null));
      dispatcher_.Spy.ClearReceivedCalls();
    }

    private void RecreateComponents(bool usePhysicsLessCollisions = false) {
      // Set up touchInputInteractor_
      {
        if (touchInputInteractor_ != null) {
          DestroyImmediate(touchInputInteractor_.gameObject);
        }

        touchInputInteractor_ = (new GameObject()).AddComponent<TouchInputInteractor.TouchInputInteractor>();
        SetPrivateField(
          touchInputInteractor_,
          "capsuleCollider_",
          touchInputInteractor_.gameObject.AddComponent<CapsuleCollider>()
        );
        SetPrivateField(touchInputInteractor_, "usePhysicsLessCollision_", usePhysicsLessCollisions);
        CallPrivateMethod(touchInputInteractor_, "Awake", new object[] { });
        CallPrivateMethod(touchInputInteractor_, "Start", new object[] { });
      }

      // Set up collision targets
      {
        DestroyImmediate(collisionTargetPriorityMinus1_);
        collisionTargetPriorityMinus1_ = CreateCollisionTarget(-1);
        DestroyImmediate(collisionTargetPriority0_);
        collisionTargetPriority0_ = CreateCollisionTarget(0);
        DestroyImmediate(collisionTargetPriority1_);
        collisionTargetPriority1_ = CreateCollisionTarget(1);
      }

      // 'Registering' the collision targets with the IInputMethodService
      registeredCollisionTargets_.Clear();
      registeredCollisionTargets_.Add(
        collisionTargetPriorityMinus1_,
        collisionTargetPriorityMinus1_.gameObject.GetComponent<TouchInputInteractorTarget>()
      );
      registeredCollisionTargets_.Add(
        collisionTargetPriority0_,
        collisionTargetPriority0_.gameObject.GetComponent<TouchInputInteractorTarget>()
      );
      registeredCollisionTargets_.Add(
        collisionTargetPriority1_,
        collisionTargetPriority1_.gameObject.GetComponent<TouchInputInteractorTarget>()
      );

      // Moving all targets out of reach
      MoveOutOfCollisionRange(collisionTargetPriorityMinus1_);
      MoveOutOfCollisionRange(collisionTargetPriority0_);
      MoveOutOfCollisionRange(collisionTargetPriority1_);
    }

    private void TriggerEnter(Collider collider) {
      MoveIntoCollisionRange(collider);
    }

    private void TriggerExit(Collider collider) {
      MoveOutOfCollisionRange(collider);
    }

    private void MoveIntoCollisionRange(Collider collider) {
      collider.gameObject.transform.SetPositionAndRotation(new Vector3(0.0f, 0.0f, 0.0f), new Quaternion());
      // This is required for methods like OverlapCapsule() to work properly.
      Physics.SyncTransforms();
    }

    private void MoveOutOfCollisionRange(Collider collider) {
      collider.gameObject.transform.SetPositionAndRotation(new Vector3(1.0f, 1.0f, 1.0f), new Quaternion());
      // This is required for methods like OverlapCapsule() to work properly.
      Physics.SyncTransforms();
    }
  }
}
