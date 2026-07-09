// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Seating;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Facebook.Workrooms.Tests.Tests.Seating {
  [OnCall(OnCallName.workrooms_core)]
  public class BreakoutsLayoutsTest {
    private BreakoutsDistributionConfigIfMaster breakoutsConfig_;

    [SetUp]
    public void OneTimeSetup() {
      breakoutsConfig_ = SeatLayoutUtils.LoadBreakoutsConfig();
    }

    [UnityTest]
    public IEnumerator TestGetNextBreakoutsLayoutIndexAndGroupSize() {
      var group = 2;
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndexAndGroupSize(
          SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS,
          7,
          ref group,
          breakoutsConfig_
        ),
        SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS
      );
      Assert.AreEqual(group, 2);

      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndexAndGroupSize(
          SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS,
          7,
          ref group,
          breakoutsConfig_
        ),
        SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS
      );
      Assert.AreEqual(group, 2);

      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndexAndGroupSize(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 16, ref group, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS
      );
      Assert.AreEqual(group, 2);

      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndexAndGroupSize(
          SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS,
          16,
          ref group,
          breakoutsConfig_
        ),
        SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS
      );
      Assert.AreEqual(group, 2);

      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndexAndGroupSize(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 17, ref group, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS
      );
      Assert.AreEqual(group, 3);

      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndexAndGroupSize(
          SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS,
          17,
          ref group,
          breakoutsConfig_
        ),
        SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS
      );
      Assert.AreEqual(group, 3);

      group = 3;
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndexAndGroupSize(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 24, ref group, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS
      );
      Assert.AreEqual(group, 3);

      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndexAndGroupSize(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 25, ref group, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS
      );
      Assert.AreEqual(group, 4);

      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndexAndGroupSize(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 32, ref group, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS
      );
      Assert.AreEqual(group, 4);

      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndexAndGroupSize(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 33, ref group, breakoutsConfig_),
        SeatLayoutIndex.INVALID_SEAT_LAYOUT
      );
      Assert.AreEqual(group, 4);
      yield return null;
    }

    [UnityTest]
    public IEnumerator TestGetNextBreakoutsLayoutIndex() {
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(1, 2, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(2, 3, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(3, 4, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(4, 2, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(7, 3, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS
      );

      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(7, 4, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(8, 2, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(8, 3, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS
      );

      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(9, 2, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(9, 3, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(10, 2, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(10, 4, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(11, 2, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(11, 4, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(12, 2, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(12, 3, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(16, 2, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(17, 2, breakoutsConfig_),
        SeatLayoutIndex.INVALID_SEAT_LAYOUT
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(24, 2, breakoutsConfig_),
        SeatLayoutIndex.INVALID_SEAT_LAYOUT
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(25, 3, breakoutsConfig_),
        SeatLayoutIndex.INVALID_SEAT_LAYOUT
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(25, 4, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(32, 4, breakoutsConfig_),
        SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextBreakoutsLayoutIndex(33, 4, breakoutsConfig_),
        SeatLayoutIndex.INVALID_SEAT_LAYOUT
      );
      yield return null;
    }

  }
}
