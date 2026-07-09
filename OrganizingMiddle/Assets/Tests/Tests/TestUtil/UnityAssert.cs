// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Tests.TestUtil {
  public class WorkroomsAssert {

    public static IEnumerator ConditionMetEventually(long timeoutMillis, Func<bool> condition) {
      var waitUntilWithTimeout = new WaitUntilWithTimeout(timeoutMillis, condition);
      yield return waitUntilWithTimeout;
      if (waitUntilWithTimeout.WasTimedOut()) {
        Assert.Fail("Timed out while waiting for condition.");
      }
    }

    public static IEnumerator ConditionMetEventually(Func<bool> condition) {
      return ConditionMetEventually(10_000, condition);
    }

    public static IEnumerator ConditionHoldFor(long durationMillis, Func<bool> condition) {
      var waitWhileWithTimeout = new WaitWhileWithTimeout(durationMillis, condition);
      yield return waitWhileWithTimeout;
      if (waitWhileWithTimeout.WasConditionBroken()) {
        Assert.Fail("Condition broke during the waiting.");
      }
    }

    private class WaitUntilWithTimeout : CustomYieldInstruction {

      private readonly Func<bool> condition_;
      private readonly long deadlineTicks_;
      private bool timedOut_ = false;

      public WaitUntilWithTimeout(long timeoutMillis, Func<bool> condition) {
        this.deadlineTicks_ = DateTime.Now.Ticks + timeoutMillis * 1000;
        this.condition_ = condition;
      }

      public override bool keepWaiting => !IsDone();

      private bool IsDone() {
        if (DateTime.Now.Ticks >= deadlineTicks_) {
          timedOut_ = true;
          return true;
        } else if (condition_.Invoke()) {
          return true;
        } else {
          return false;
        }
      }

      public bool WasTimedOut() {
        return timedOut_;
      }
    }

    private class WaitWhileWithTimeout : CustomYieldInstruction {

      private readonly Func<bool> condition_;
      private readonly long deadlineTicks_;
      private bool conditionBroken_ = false;

      public WaitWhileWithTimeout(long timeoutMillis, Func<bool> condition) {
        this.deadlineTicks_ = DateTime.Now.Ticks + timeoutMillis * 1000;
        this.condition_ = condition;
      }

      public override bool keepWaiting {
        get {
          if (!condition_.Invoke()) {
            conditionBroken_ = true;
            return false;
          } else if (DateTime.Now.Ticks >= deadlineTicks_) {
            return false;
          } else {
            return true;
          }
        }
      }

      public bool WasConditionBroken() {
        return conditionBroken_;
      }
    }
  }

}
