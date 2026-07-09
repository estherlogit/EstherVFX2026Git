// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections;
using Facebook.SocialVR.Core.Inputs;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.OnCall;
using Facebook.SocialVR.Core.Player;
using Facebook.SocialVR.Core.Services;
using Facebook.Workrooms.InputDevice;
using Facebook.Workrooms.LaserPointer;
using Facebook.Workrooms.MRRoom;
using Facebook.Xplat.Events;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Tests.Tests.TestUtil.WorkroomsAssert;

namespace Tests.Tests.MixedReality {

  [OnCall(OnCallName.workrooms_mixed_reality)]
  public class MrRoomSelectableObjectTest {
    private MrRoomSelectableObject selectableObject_;
    private GameObject attachedGameObject_;
    private IServiceContainer container_;
    private IPlayerDriver playerDriver_;
    private Transform rightHandTransform_;

    [SetUp]
    public void SetUp() {
      container_ = ServiceLocator.CreateServiceContainer(SceneManager.GetActiveScene().path);
      playerDriver_ = Substitute.For<IPlayerDriver>();
      container_.Bind<IPlayerDriver>().To(playerDriver_);
      container_.Bind<IInputMethodService>().To(Substitute.For<IInputMethodService>());
      container_.Bind<IDispatcher>().To(Substitute.For<IDispatcher>());
      container_.Bind<ILogService>().To(Substitute.For<ILogService>());

      rightHandTransform_ = new GameObject().transform;
      rightHandTransform_.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
      playerDriver_.rightHand.Returns(rightHandTransform_);

      attachedGameObject_ = new GameObject();
      selectableObject_ = attachedGameObject_.AddComponent<MrRoomSelectableObject>();
      selectableObject_.Target = attachedGameObject_.transform;
      selectableObject_.IsDraggable = true;
    }

    [UnityTest]
    public IEnumerator TestDragAtCenter() {
      // Arrange
      rightHandTransform_.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
      IWorkroomsLaserPointer laserPointer = Substitute.For<IWorkroomsLaserPointer>();
      laserPointer.Handedness.Returns(Handedness.RIGHT);
      laserPointer.EndPoint.Returns(Vector3.zero);

      // Act and assert
      // Grab object.
      selectableObject_.OnLaserPointerTriggerDownEvent(
        new LaserPointerTriggerDownEvent { LaserPointer = laserPointer }
      );
      yield return ConditionMetEventually(() => selectableObject_.transform.position == Vector3.zero);

      // Move hand.
      rightHandTransform_.SetPositionAndRotation(Vector3.one, Quaternion.identity);
      yield return ConditionMetEventually(() => { return selectableObject_.transform.position == Vector3.one; });

      // Release object.
      selectableObject_.OnLaserPointerTriggerUpEvent(new LaserPointerTriggerUpEvent { LaserPointer = laserPointer });
      yield return ConditionMetEventually(() => selectableObject_.transform.position == Vector3.one);

      // Move hand again.
      rightHandTransform_.SetPositionAndRotation(Vector3.back, Quaternion.identity);
      yield return ConditionHoldFor(1000, () => selectableObject_.transform.position == Vector3.one);
    }

  }
}
