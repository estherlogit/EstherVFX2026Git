// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Whiteboard;
using NUnit.Framework;

namespace Tests.UnitTests.CollabCanvas {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class IsUpdatingStateProviderTest {
    private class MockIsUpdatingStateProvider : IIsUpdatingStateProvider {
      public event Action IsUpdatingStateChanged;
      public bool IsUpdating { get; set; } = false;

      public void Trigger() {
        IsUpdatingStateChanged?.Invoke();
      }
    }

    private MockIsUpdatingStateProvider provider1_;
    private MockIsUpdatingStateProvider provider2_;
    private MockIsUpdatingStateProvider provider3_;

    [SetUp]
    public void Setup() {
      provider1_ = new();
      provider2_ = new();
      provider3_ = new();
    }

    [Test]
    public void MultiIsUpdatingProviderTriggersForAnySubProviderCalled() {
      var timesActionCalled = 0;

      var multiProvider = new MultiIsUpdatingStateProvider(provider1_, provider2_, provider3_);
      multiProvider.IsUpdatingStateChanged += delegate {
        timesActionCalled++;
      };

      Assert.IsFalse(multiProvider.IsUpdating);
      Assert.AreEqual(0, timesActionCalled);

      provider1_.Trigger();
      Assert.AreEqual(1, timesActionCalled);

      provider2_.Trigger();
      Assert.AreEqual(2, timesActionCalled);

      provider3_.Trigger();
      Assert.AreEqual(3, timesActionCalled);

      // after all of the above, we are still not in updating state as none of the providers indicate so
      Assert.IsFalse(multiProvider.IsUpdating);

      provider1_.IsUpdating = true;
      Assert.IsTrue(multiProvider.IsUpdating);

      provider1_.IsUpdating = false;
      Assert.IsFalse(multiProvider.IsUpdating);

      provider2_.IsUpdating = true;
      Assert.IsTrue(multiProvider.IsUpdating);

      provider3_.IsUpdating = true;
      Assert.IsTrue(multiProvider.IsUpdating);
    }
  }
}
