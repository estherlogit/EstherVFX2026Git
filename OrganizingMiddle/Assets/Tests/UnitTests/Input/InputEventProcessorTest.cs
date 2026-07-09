// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using Facebook.SocialVR.Core.Inputs;
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
using Facebook.Workrooms.Input;
using Facebook.Workrooms.LaserPointer;
using Facebook.Workrooms.Raycaster;
using Object = UnityEngine.Object;

namespace Facebook.Workrooms.Tests.UnitTests {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class InputEventProcessorTest {
    private GameObject targetObj_;
    private Collider targetCollider_;
    private IServiceContainer container_;
    private IDispatcher dispatcher_;
    private InputEventProcessor inputEventProcessor_;
    private IWorkroomsLaserPointer leftLaserPointer_;
    private IWorkroomsLaserPointer rightLaserPointer_;
    private ITouchInputInteractor leftTouchInteractor_;
    private ITouchInputInteractor rightTouchInteractor_;

    [SetUp]
    public void Setup() {
      targetObj_ = new GameObject();
      targetCollider_ = targetObj_.AddComponent<BoxCollider>();

      container_ = ServiceLocator.CreateServiceContainer(SceneManager.GetActiveScene().path);

      dispatcher_ = new DispatcherSpyDecorator(new Dispatcher(), Substitute.For<IDispatcher>());
      container_.Bind<IDispatcher>().To(dispatcher_);

      var logger = Substitute.For<ILogService>();
      container_.Bind<ILogService>().To(logger);

      leftLaserPointer_ = InitLaser(Handedness.LEFT);
      rightLaserPointer_ = InitLaser(Handedness.RIGHT);
      leftTouchInteractor_ = InitTouchInteractor(Handedness.LEFT);
      rightTouchInteractor_ = InitTouchInteractor(Handedness.RIGHT);
      inputEventProcessor_ = new InputEventProcessor(
        dispatcher_,
        logger.GetLog("input event processor"),
        targetCollider_
      );
    }

    [TearDown]
    public void Cleanup() {
      ServiceLocator.Clear();
      container_.Clear();
      ServiceLocator.RemoveServiceContainer(SceneManager.GetActiveScene().path);
      Object.DestroyImmediate(targetObj_);
    }

    [Test]
    public void BasicLaserEventProcessing() {
      var startEventCount = 0;
      inputEventProcessor_.InputStart += (processor, interactor) => startEventCount++;
      var updateEventCount = 0;
      inputEventProcessor_.InputUpdate += (processor, interactor) => updateEventCount++;
      var endEventCount = 0;
      inputEventProcessor_.InputEnd += (processor, interactor) => endEventCount++;
      leftLaserPointer_.CurrentHitCollider.Returns(targetCollider_);
      leftLaserPointer_.CurrentInputPosition.Returns(Vector3.zero);

      // Trigger down we receive a start and update
      dispatcher_.Dispatch(GetLaserEvent(EventType.TRIGGER_DOWN, leftLaserPointer_));
      Assert.AreEqual(1, startEventCount);
      Assert.AreEqual(1, updateEventCount);
      Assert.AreEqual(0, endEventCount);

      // On Over, we receive another update
      dispatcher_.Dispatch(GetLaserEvent(EventType.OVER, leftLaserPointer_));
      Assert.AreEqual(1, startEventCount);
      Assert.AreEqual(2, updateEventCount);
      Assert.AreEqual(0, endEventCount);

      // On Exit, we keep input active, waiting for the trigger up event, and issue another input update event
      dispatcher_.Dispatch(GetLaserEvent(EventType.EXIT, leftLaserPointer_));
      Assert.AreEqual(1, startEventCount);
      Assert.AreEqual(3, updateEventCount);
      Assert.AreEqual(0, endEventCount);

      // FALSE = trigger release
      inputEventProcessor_.LaserTriggerActionHandler(Handedness.RIGHT, false);
      // no exit event yet, wrong hand
      Assert.AreEqual(3, updateEventCount);
      Assert.AreEqual(0, endEventCount);

      // Finally, correct hand trigger up, receive the end event
      inputEventProcessor_.LaserTriggerActionHandler(Handedness.LEFT, false);
      Assert.AreEqual(3, updateEventCount);
      Assert.AreEqual(1, endEventCount);
    }

    [Test]
    public void BasicTouchEventProcessing() {
      var startEventCount = 0;
      inputEventProcessor_.InputStart += (processor, interactor) => startEventCount++;
      var updateEventCount = 0;
      inputEventProcessor_.InputUpdate += (processor, interactor) => updateEventCount++;
      var endEventCount = 0;
      inputEventProcessor_.InputEnd += (processor, interactor) => endEventCount++;

      leftTouchInteractor_.CurrentHitCollider.Returns(targetCollider_);
      leftTouchInteractor_.CurrentInputPosition.Returns(Vector3.zero);

      // Trigger down we receive a start and update
      dispatcher_.Dispatch(GetTouchEvent(EventType.ENTER, leftTouchInteractor_));
      Assert.AreEqual(1, startEventCount);
      Assert.AreEqual(1, updateEventCount);
      Assert.AreEqual(0, endEventCount);

      // On Over, we receive another update
      dispatcher_.Dispatch(GetTouchEvent(EventType.OVER, leftTouchInteractor_));
      Assert.AreEqual(1, startEventCount);
      Assert.AreEqual(2, updateEventCount);
      Assert.AreEqual(0, endEventCount);

      dispatcher_.Dispatch(GetTouchEvent(EventType.EXIT, rightTouchInteractor_));
      // no exit event yet, wrong hand
      Assert.AreEqual(2, updateEventCount);
      Assert.AreEqual(0, endEventCount);

      // Finally, correct hand stops touching, receive the end event
      leftTouchInteractor_.CurrentHitCollider.Returns((Collider)null);
      dispatcher_.Dispatch(GetTouchEvent(EventType.EXIT, leftTouchInteractor_));
      Assert.AreEqual(2, updateEventCount);
      Assert.AreEqual(1, endEventCount);
    }

    [Test]
    public void MultipleLaserTriggerDownCauseSingleInputStart() {
      var startEventCount = 0;
      inputEventProcessor_.InputStart += (processor, interactor) => startEventCount++;
      leftLaserPointer_.CurrentHitCollider.Returns(targetCollider_);
      dispatcher_.Dispatch(GetLaserEvent(EventType.TRIGGER_DOWN, leftLaserPointer_));
      // This trigger event is ignored because left laser is already active
      dispatcher_.Dispatch(GetLaserEvent(EventType.TRIGGER_DOWN, rightLaserPointer_));
      Assert.AreEqual(1, startEventCount);
    }

    [Test]
    public void LaserEnterEventsInvokeHoverOverImmediately() {
      var hoverOverEventCount = 0;
      inputEventProcessor_.HoverOver += (processor, interactor) => hoverOverEventCount++;
      leftLaserPointer_.CurrentHitCollider.Returns(targetCollider_);
      leftLaserPointer_.CurrentInputPosition.Returns(Vector3.zero);
      dispatcher_.Dispatch(GetLaserEvent(EventType.ENTER, leftLaserPointer_));
      Assert.AreEqual(1, hoverOverEventCount);
    }

    [Test]
    public void LaserTriggerUpAndGenericRaycastTriggerUpDoNotCauseMultipleInputEndEvents() {
      var endEventCount = 0;
      inputEventProcessor_.InputEnd += (processor, interactor) => endEventCount++;
      leftLaserPointer_.CurrentHitCollider.Returns(targetCollider_);
      dispatcher_.Dispatch(GetLaserEvent(EventType.TRIGGER_DOWN, leftLaserPointer_));

      // Laser trigger up cause end event to trigger
      dispatcher_.Dispatch(GetLaserEvent(EventType.TRIGGER_UP, leftLaserPointer_));
      Assert.AreEqual(1, endEventCount);

      // Later generic raycaster event doesn't invoke end event again
      inputEventProcessor_.LaserTriggerActionHandler(Handedness.LEFT, false);
      Assert.AreEqual(1, endEventCount);
    }

    [Test]
    public void TouchInputEventsInvokeHoverEvents() {
      var hoverOverEventCount = 0;
      inputEventProcessor_.HoverOver += (processor, interactor) => hoverOverEventCount++;
      var hoverExitEventCount = 0;
      inputEventProcessor_.HoverExit += (processor, interactor) => hoverExitEventCount++;
      leftTouchInteractor_.CurrentHitCollider.Returns(targetCollider_);
      leftTouchInteractor_.CurrentInputPosition.Returns(Vector3.zero);

      dispatcher_.Dispatch(GetTouchEvent(EventType.ENTER, leftTouchInteractor_));
      Assert.AreEqual(1, hoverOverEventCount);
      Assert.AreEqual(0, hoverExitEventCount);

      dispatcher_.Dispatch(GetTouchEvent(EventType.OVER, leftTouchInteractor_));
      Assert.AreEqual(2, hoverOverEventCount);
      Assert.AreEqual(0, hoverExitEventCount);

      dispatcher_.Dispatch(GetTouchEvent(EventType.EXIT, leftTouchInteractor_));
      Assert.AreEqual(2, hoverOverEventCount);
      Assert.AreEqual(1, hoverExitEventCount);
    }

    [Test]
    public void DontInvokeHoverExitIfDifferentInteractorActive() {
      int hoverExitEventCount = 0, hoverOverEventCount = 0;
      inputEventProcessor_.HoverOver += (processor, interactor) => hoverOverEventCount++;
      inputEventProcessor_.HoverExit += (processor, interactor) => hoverExitEventCount++;

      leftTouchInteractor_.CurrentHitCollider.Returns(targetCollider_);
      leftTouchInteractor_.CurrentInputPosition.Returns(Vector3.zero);
      dispatcher_.Dispatch(GetTouchEvent(EventType.ENTER, leftTouchInteractor_));
      Assert.AreEqual(1, hoverOverEventCount);
      Assert.AreEqual(0, hoverExitEventCount);

      // Input started for the touch interactor, so hover events ignored
      leftLaserPointer_.CurrentHitCollider.Returns(targetCollider_);
      dispatcher_.Dispatch(GetLaserEvent(EventType.ENTER, leftLaserPointer_));
      Assert.AreEqual(1, hoverOverEventCount);
      Assert.AreEqual(0, hoverExitEventCount);
      dispatcher_.Dispatch(GetLaserEvent(EventType.EXIT, leftLaserPointer_));
      Assert.AreEqual(1, hoverOverEventCount);
      Assert.AreEqual(0, hoverExitEventCount);

      // Process hover events once the interactor exits
      dispatcher_.Dispatch(GetTouchEvent(EventType.EXIT, leftTouchInteractor_));
      Assert.AreEqual(1, hoverOverEventCount);
      Assert.AreEqual(1, hoverExitEventCount);
    }

    private IWorkroomsLaserPointer InitLaser(Handedness handedness) {
      var laser = Substitute.For<IWorkroomsLaserPointer>();
      laser.IsTriggerDown.Returns(false);
      laser.Handedness.Returns(handedness);
      return laser;
    }

    private ITouchInputInteractor InitTouchInteractor(Handedness handedness) {
      var laser = Substitute.For<ITouchInputInteractor>();
      laser.Handedness.Returns(handedness);
      return laser;
    }

    private ISourceEvent<GameObject> GetTouchEvent(EventType eventType, ITouchInputInteractor interactor) {
      switch (eventType) {
        case EventType.ENTER:
          var enterEvent = new TouchInputInteractorEnterEvent();
          enterEvent.Init(targetObj_, null, interactor.Handedness, false, interactor);
          return enterEvent;
        case EventType.OVER:
          var overEvent = new TouchInputInteractorOverEvent();
          overEvent.Init(targetObj_, null, interactor.Handedness, interactor);
          return overEvent;
        case EventType.EXIT:
          var exitEvent = new TouchInputInteractorExitEvent();
          exitEvent.Init(targetObj_, interactor.Handedness, interactor);
          return exitEvent;
        default:
          throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
      }
    }

    private ISourceEvent<GameObject> GetLaserEvent(EventType eventType, IWorkroomsLaserPointer interactor) {
      switch (eventType) {
        case EventType.ENTER:
          return new LaserPointerEnterEvent() {
            LaserPointer = interactor,
            Source = targetObj_
          };
        case EventType.OVER:
          return new LaserPointerOverEvent() {
            LaserPointer = interactor,
            Source = targetObj_
          };
        case EventType.EXIT:
          return new LaserPointerExitEvent() {
            LaserPointer = interactor,
            Source = targetObj_
          };
        case EventType.TRIGGER_DOWN:
          return new LaserPointerTriggerDownEvent() {
            LaserPointer = interactor,
            Source = targetObj_
          };
        case EventType.TRIGGER_UP:
          return new LaserPointerTriggerUpEvent() {
            LaserPointer = interactor,
            Source = targetObj_
          };
        default:
          throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
      }
    }

    private enum EventType {
      ENTER,
      OVER,
      EXIT,
      TRIGGER_DOWN,
      TRIGGER_UP
    }
  }
}
