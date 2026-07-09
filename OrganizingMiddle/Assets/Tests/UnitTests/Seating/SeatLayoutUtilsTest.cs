// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Facebook.SocialVR.Core.OnCall;
using Facebook.SocialVR.Core.Utils;
using Facebook.Workrooms.Seating;
using NUnit.Framework;

namespace Facebook.Workrooms.Tests.UnitTests.Seating {
  [OnCall(OnCallName.workrooms_core)]
  public class SeatLayoutUtilsTest {

    [Test]
    public void TestGetNextLayoutIndex() {
      Assert.AreEqual(
        SeatLayoutUtils.GetNextLayoutIndex(SeatLayoutIndex.INVALID_SEAT_LAYOUT),
        SeatLayoutIndex.INVALID_SEAT_LAYOUT
      );

      Assert.AreEqual(
        SeatLayoutUtils.GetNextLayoutIndex(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS),
        SeatLayoutIndex.CONFERENCE_LAYOUT_6_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextLayoutIndex(SeatLayoutIndex.CONFERENCE_LAYOUT_16_SEATS),
        SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS
      );

      Assert.AreEqual(
        SeatLayoutUtils.GetNextLayoutIndex(SeatLayoutIndex.PRESENTATION_LAYOUT_6_SEATS),
        SeatLayoutIndex.PRESENTATION_LAYOUT_10_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextLayoutIndex(SeatLayoutIndex.PRESENTATION_LAYOUT_16_SEATS),
        SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextLayoutIndex(SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS),
        SeatLayoutIndex.INVALID_SEAT_LAYOUT
      );

      Assert.AreEqual(
        SeatLayoutUtils.GetNextLayoutIndex(SeatLayoutIndex.CONVERSATION_LAYOUT_4_SEATS),
        SeatLayoutIndex.CONVERSATION_LAYOUT_6_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextLayoutIndex(SeatLayoutIndex.CONVERSATION_LAYOUT_16_SEATS),
        SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextLayoutIndex(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS),
        SeatLayoutIndex.INVALID_SEAT_LAYOUT
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextLayoutIndex(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS),
        SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextLayoutIndex(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS),
        SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextLayoutIndex(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS),
        SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetNextLayoutIndex(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS),
        SeatLayoutIndex.INVALID_SEAT_LAYOUT
      );
    }

    [Test]
    public void TestGetLayoutIndexFromModeAndMinSeatCount() {
      #region Conference Layouts

      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Conference, 4),
        SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Conference, 10),
        SeatLayoutIndex.CONFERENCE_LAYOUT_10_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Conference, 16),
        SeatLayoutIndex.CONFERENCE_LAYOUT_16_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Conference, 32),
        SeatLayoutIndex.INVALID_SEAT_LAYOUT
      );

      #endregion

      #region Presentation Layouts

      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Presentation, 4),
        SeatLayoutIndex.PRESENTATION_LAYOUT_6_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Presentation, 10),
        SeatLayoutIndex.PRESENTATION_LAYOUT_10_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Presentation, 11),
        SeatLayoutIndex.PRESENTATION_LAYOUT_16_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Presentation, 32),
        SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Presentation, 33),
        SeatLayoutIndex.INVALID_SEAT_LAYOUT
      );

      #endregion

      #region Conversation Layouts

      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Conversation, 4),
        SeatLayoutIndex.CONVERSATION_LAYOUT_4_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Conversation, 10),
        SeatLayoutIndex.CONVERSATION_LAYOUT_10_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Conversation, 16),
        SeatLayoutIndex.CONVERSATION_LAYOUT_16_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Conversation, 32),
        SeatLayoutIndex.INVALID_SEAT_LAYOUT
      );

      #endregion

      #region Breakouts Layouts

      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Breakout, 6),
        SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Breakout, 12),
        SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Breakout, 16),
        SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Breakout, 20),
        SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Breakout, 24),
        SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS
      );
      Assert.AreEqual(
        SeatLayoutUtils.GetLayoutIndexFromModeAndMinSeatCount(RoomLayouts.Breakout, 32),
        SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS
      );

      #endregion
    }

    [Test]
    public void TestLayoutShrinkingPossibility() {
      #region Conference Layouts

      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS, 4));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_6_SEATS, 4));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_8_SEATS, 4));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_10_SEATS, 4));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_12_SEATS, 4));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_14_SEATS, 4));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_16_SEATS, 4));

      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS, 6));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_6_SEATS, 6));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_8_SEATS, 6));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_10_SEATS, 6));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_12_SEATS, 6));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_14_SEATS, 6));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_16_SEATS, 6));

      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS, 8));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_6_SEATS, 8));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_8_SEATS, 8));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_10_SEATS, 8));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_12_SEATS, 8));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_14_SEATS, 8));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_16_SEATS, 8));

      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS, 10));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_6_SEATS, 10));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_8_SEATS, 10));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_10_SEATS, 10));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_12_SEATS, 10));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_14_SEATS, 10));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_16_SEATS, 10));

      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS, 12));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_6_SEATS, 12));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_8_SEATS, 12));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_10_SEATS, 12));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_12_SEATS, 12));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_14_SEATS, 12));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_16_SEATS, 12));

      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS, 14));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_6_SEATS, 14));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_8_SEATS, 14));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_10_SEATS, 14));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_12_SEATS, 14));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_14_SEATS, 14));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_16_SEATS, 14));

      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS, 16));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_6_SEATS, 16));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_8_SEATS, 16));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_10_SEATS, 16));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_12_SEATS, 16));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_14_SEATS, 16));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.CONFERENCE_LAYOUT_16_SEATS, 16));

      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 15));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 16));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 17));

      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS, 1));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 12));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 20));
      Assert.True(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 24));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 13));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 21));
      Assert.False(SeatLayoutUtils.IsLayoutShrinkingPossible(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 25));

      #endregion
    }

    [Test]
    public void TestIsLayoutFull() {
      Assert.IsFalse(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS, 1, 4, false)
      );
      Assert.IsFalse(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS, 3, 4, true)
      );
      Assert.IsFalse(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS, 4, 4, true)
      );
      Assert.IsFalse(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.CONFERENCE_LAYOUT_16_SEATS, 4, 16, true)
      );
      Assert.IsFalse(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.CONFERENCE_LAYOUT_16_SEATS, 16, 16, true)
      );
      Assert.IsTrue(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.CONFERENCE_LAYOUT_16_SEATS, 16, 16, false)
      );

      Assert.IsFalse(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.PRESENTATION_LAYOUT_6_SEATS, 1, 6, true)
      );
      Assert.IsFalse(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.PRESENTATION_LAYOUT_6_SEATS, 6, 6, true)
      );
      Assert.IsFalse(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.PRESENTATION_LAYOUT_16_SEATS, 16, 16, true)
      );
      Assert.IsTrue(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.PRESENTATION_LAYOUT_16_SEATS, 16, 16, false)
      );
      Assert.IsTrue(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS, 32, 32, true)
      );

      Assert.IsFalse(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.CONVERSATION_LAYOUT_4_SEATS, 1, 4, true)
      );
      Assert.IsFalse(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.CONVERSATION_LAYOUT_4_SEATS, 3, 4, true)
      );
      Assert.IsFalse(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.CONVERSATION_LAYOUT_4_SEATS, 4, 4, false)
      );
      Assert.IsFalse(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.CONVERSATION_LAYOUT_16_SEATS, 4, 16, false)
      );
      Assert.IsFalse(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.CONVERSATION_LAYOUT_16_SEATS, 16, 16, true)
      );
      Assert.IsTrue(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.CONVERSATION_LAYOUT_16_SEATS, 16, 16, false)
      );

      Assert.IsTrue(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 16, 16, false)
      );
      Assert.IsFalse(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS, 12, 12, true)
      );
      Assert.IsFalse(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 20, 20, true)
      );
      Assert.IsFalse(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 24, 24, true)
      );
      Assert.IsTrue(
        SeatLayoutUtils.IsLayoutFullAndNotExpandable(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 32, 32, true)
      );
    }

    [Test]
    public void TestIsLecternSeat() {
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS, 0));
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS, 4));

      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.CONVERSATION_LAYOUT_4_SEATS, 0));
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.CONVERSATION_LAYOUT_4_SEATS, 4));

      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_6_SEATS, 0));
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_6_SEATS, 1));
      Assert.IsTrue(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_6_SEATS, 6));

      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_10_SEATS, 0));
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_10_SEATS, 1));
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_10_SEATS, 6));
      Assert.IsTrue(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_10_SEATS, 10));

      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_16_SEATS, 0));
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_16_SEATS, 1));
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_16_SEATS, 6));
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_16_SEATS, 10));
      Assert.IsTrue(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_16_SEATS, 16));

      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS, 0));
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS, 1));
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS, 6));
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS, 10));
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS, 16));
      Assert.IsTrue(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS, 32));

      // Breakouts moderators
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 20));
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS, 15));
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 25));
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 30));
      Assert.IsFalse(SeatLayoutUtils.IsLecternSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 40));
    }

    [Test]
    public void TestIsModerator() {
      Assert.IsTrue(SeatLayoutUtils.IsModeratorSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 20));
      Assert.IsFalse(SeatLayoutUtils.IsModeratorSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 21));
      Assert.IsFalse(SeatLayoutUtils.IsModeratorSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 19));
      Assert.IsFalse(SeatLayoutUtils.IsModeratorSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 0));
      Assert.IsTrue(SeatLayoutUtils.IsModeratorSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS, 15));
      Assert.IsTrue(SeatLayoutUtils.IsModeratorSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 25));
      Assert.IsTrue(SeatLayoutUtils.IsModeratorSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 30));
      Assert.IsTrue(SeatLayoutUtils.IsModeratorSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 40));

      Assert.IsFalse(SeatLayoutUtils.IsModeratorSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_10_SEATS, 10));
      Assert.IsFalse(SeatLayoutUtils.IsModeratorSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_16_SEATS, 16));
      Assert.IsFalse(SeatLayoutUtils.IsModeratorSeat(SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS, 32));
      Assert.IsFalse(SeatLayoutUtils.IsModeratorSeat(SeatLayoutIndex.CONFERENCE_LAYOUT_16_SEATS, 16));
      Assert.IsFalse(SeatLayoutUtils.IsModeratorSeat(SeatLayoutIndex.CONVERSATION_LAYOUT_16_SEATS, 16));
    }

    [Test]
    public void TestGetCluster() {
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 0), 0);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 3), 0);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 4), 1);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 7), 1);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 8), 2);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 11), 2);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 12), 3);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 15), 3);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 16), 0);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 17), 1);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 18), 2);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 19), 3);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 20), -1);

      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS, 10), 2);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 16), 4);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 20), 5);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 30), 7);

      // Observers
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 20), 0);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 22), 2);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 24), 4);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 28), 4);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 29), 5);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 32), 0);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 39), 7);

      // Moderators
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS, 15), -1);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 26), -1);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 31), -1);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 40), -1);

      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS, 0), -1);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.CONVERSATION_LAYOUT_4_SEATS, 3), -1);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.PRESENTATION_LAYOUT_10_SEATS, 19), -1);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS, 20), -1);
      Assert.AreEqual(SeatLayoutUtils.GetCluster(SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS, 32), -1);
    }

    [Test]
    public void TestGetNumClusters() {
      Assert.AreEqual(SeatLayoutUtils.GetNumClusters(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS), 4);
      Assert.AreEqual(SeatLayoutUtils.GetNumClusters(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS), 3);
      Assert.AreEqual(SeatLayoutUtils.GetNumClusters(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS), 5);
      Assert.AreEqual(SeatLayoutUtils.GetNumClusters(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS), 6);
      Assert.AreEqual(SeatLayoutUtils.GetNumClusters(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS), 8);
    }

    [Test]
    public void TestGetExtraSeatCount() {
      Assert.AreEqual(SeatLayoutUtils.GetExtraSeatCount(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS), 5);
      Assert.AreEqual(SeatLayoutUtils.GetExtraSeatCount(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS), 4);
      Assert.AreEqual(SeatLayoutUtils.GetExtraSeatCount(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS), 6);
      Assert.AreEqual(SeatLayoutUtils.GetExtraSeatCount(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS), 7);
      Assert.AreEqual(SeatLayoutUtils.GetExtraSeatCount(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS), 9);

      Assert.AreEqual(SeatLayoutUtils.GetExtraSeatCount(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS), 0);
      Assert.AreEqual(SeatLayoutUtils.GetExtraSeatCount(SeatLayoutIndex.CONVERSATION_LAYOUT_4_SEATS), 0);
      Assert.AreEqual(SeatLayoutUtils.GetExtraSeatCount(SeatLayoutIndex.PRESENTATION_LAYOUT_10_SEATS), 1);
      Assert.AreEqual(SeatLayoutUtils.GetExtraSeatCount(SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS), 1);
      Assert.AreEqual(SeatLayoutUtils.GetExtraSeatCount(SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS), 1);
    }

    [Test]
    public void TestGetVCCluster() {
      Assert.AreEqual(SeatLayoutUtils.GetVCCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS), 0);
      Assert.AreEqual(SeatLayoutUtils.GetVCCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS), 1);
      Assert.AreEqual(SeatLayoutUtils.GetVCCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS), 1);
      Assert.AreEqual(SeatLayoutUtils.GetVCCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS), 1);
      Assert.AreEqual(SeatLayoutUtils.GetVCCluster(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS), 1);

      Assert.AreEqual(SeatLayoutUtils.GetVCCluster(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS), -1);
      Assert.AreEqual(SeatLayoutUtils.GetVCCluster(SeatLayoutIndex.CONVERSATION_LAYOUT_4_SEATS), -1);
      Assert.AreEqual(SeatLayoutUtils.GetVCCluster(SeatLayoutIndex.PRESENTATION_LAYOUT_10_SEATS), -1);
      Assert.AreEqual(SeatLayoutUtils.GetVCCluster(SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS), -1);
      Assert.AreEqual(SeatLayoutUtils.GetVCCluster(SeatLayoutIndex.PRESENTATION_LAYOUT_32_SEATS), -1);
    }

    [Test]
    public void TestIsVCClusterSeat() {
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 0));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 1));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 2));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 3));
      Assert.IsTrue(
        SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 16)
      ); // Observer spot in VC cluster

      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS, 4));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS, 5));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS, 6));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS, 7));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 4));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 5));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 6));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 7));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 4));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 5));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 6));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 7));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 4));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 5));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 6));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 7));

      // Observer spots in VC cluster
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS, 13));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 21));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 25));
      Assert.IsTrue(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 33));

      // Moderators seats
      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS, 15));
      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 25));
      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 30));
      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 40));

      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS, 0));
      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 1));
      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 2));
      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 3));

      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 4));
      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 5));
      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 6));
      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 7));

      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 4));
      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 5));
      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS, 0));
      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS, 3));
      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 17));
      Assert.IsFalse(SeatLayoutUtils.IsVCClusterSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 20));
    }

    [Test]
    public void TestIsObserverSeat() {
      Assert.IsFalse(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS, 11));
      Assert.IsTrue(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS, 12));
      Assert.IsFalse(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS, 15));
      Assert.IsFalse(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS, 16));
      Assert.IsFalse(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.CONFERENCE_LAYOUT_12_SEATS, 12));

      Assert.IsFalse(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 10));
      Assert.IsFalse(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 15));
      Assert.IsTrue(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 16));
      Assert.IsTrue(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 17));
      Assert.IsFalse(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 15));
      Assert.IsTrue(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_16_SEATS, 16));

      Assert.IsFalse(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 19));
      Assert.IsTrue(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 22));
      Assert.IsTrue(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 24));
      Assert.IsFalse(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS, 25));

      Assert.IsFalse(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 23));
      Assert.IsTrue(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 26));
      Assert.IsTrue(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 28));
      Assert.IsFalse(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS, 30));

      Assert.IsFalse(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 31));
      Assert.IsTrue(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 32));
      Assert.IsTrue(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 39));
      Assert.IsFalse(SeatLayoutUtils.IsObserverSeat(SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS, 40));
    }

    [Test]
    public void TestKeepObserverCluster() {
      Assert.AreEqual(
        SeatLayoutUtils.KeepObserverCluster(
          SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS,
          SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS,
          12
        ),
        20
      );
      Assert.AreEqual(
        SeatLayoutUtils.KeepObserverCluster(
          SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS,
          SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS,
          13
        ),
        21
      );
      Assert.AreEqual(
        SeatLayoutUtils.KeepObserverCluster(
          SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS,
          SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS,
          14
        ),
        26
      );
      Assert.AreEqual(
        SeatLayoutUtils.KeepObserverCluster(
          SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS,
          SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS,
          29
        ),
        37
      );

      Assert.AreEqual(
        SeatLayoutUtils.KeepObserverCluster(
          SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS,
          SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS,
          15
        ),
        SeatLayoutUtils.INVALID_SEAT_ID
      );

      Assert.AreEqual(
        SeatLayoutUtils.KeepObserverCluster(
          SeatLayoutIndex.CONFERENCE_LAYOUT_12_SEATS,
          SeatLayoutIndex.BREAKOUT_LAYOUT_20_SEATS,
          12
        ),
        SeatLayoutUtils.INVALID_SEAT_ID
      );

      Assert.AreEqual(
        SeatLayoutUtils.KeepObserverCluster(
          SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS,
          SeatLayoutIndex.CONVERSATION_LAYOUT_16_SEATS,
          12
        ),
        SeatLayoutUtils.INVALID_SEAT_ID
      );

      Assert.AreEqual(
        SeatLayoutUtils.KeepObserverCluster(
          SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS,
          SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS,
          12
        ),
        12
      );

      Assert.AreEqual(
        SeatLayoutUtils.KeepObserverCluster(
          SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS,
          SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS,
          35
        ),
        SeatLayoutUtils.INVALID_SEAT_ID
      );

      Assert.AreEqual(
        SeatLayoutUtils.KeepObserverCluster(
          SeatLayoutIndex.BREAKOUT_LAYOUT_32_SEATS,
          SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS,
          32
        ),
        12
      );

      Assert.AreEqual(
        SeatLayoutUtils.KeepObserverCluster(
          SeatLayoutIndex.BREAKOUT_LAYOUT_24_SEATS,
          SeatLayoutIndex.BREAKOUT_LAYOUT_12_SEATS,
          26
        ),
        14
      );
    }

    [Test]
    public void TestSeatReassignmentAlgorithm() {
      // One person is at seat 0 => expectedly no change after reassignment.
      Assert.AreEqual(
        "0:1;",
        SeatAssignmentsToString(
          SeatLayoutUtils.ReassignSeatsKeepingOrderAndMinimizingMovements(
            SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS,
            SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS,
            StringToSeatAssignments("0:1"),
            null
          )
        )
      );

      // One person is at seat 1 => expectedly no change after reassignment.
      Assert.AreEqual(
        "0:-1;1:1;",
        SeatAssignmentsToString(
          SeatLayoutUtils.ReassignSeatsKeepingOrderAndMinimizingMovements(
            SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS,
            SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS,
            StringToSeatAssignments("1:1"),
            null
          )
        )
      );

      // One person is at seat 2 => expectedly no change after reassignment.
      Assert.AreEqual(
        "0:-1;1:-1;2:1;",
        SeatAssignmentsToString(
          SeatLayoutUtils.ReassignSeatsKeepingOrderAndMinimizingMovements(
            SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS,
            SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS,
            StringToSeatAssignments("2:1"),
            null
          )
        )
      );

      // One person is at seat 3 => expectedly no change after reassignment.
      Assert.AreEqual(
        "0:-1;1:-1;2:-1;3:1;",
        SeatAssignmentsToString(
          SeatLayoutUtils.ReassignSeatsKeepingOrderAndMinimizingMovements(
            SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS,
            SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS,
            StringToSeatAssignments("3:1"),
            null
          )
        )
      );

      // Two persons are at seat 0 and seat 2 => expectedly no change after reassignment.
      Assert.AreEqual(
        "0:1;1:-1;2:2;",
        SeatAssignmentsToString(
          SeatLayoutUtils.ReassignSeatsKeepingOrderAndMinimizingMovements(
            SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS,
            SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS,
            StringToSeatAssignments("0:1;1:-1;2:2"),
            null
          )
        )
      );

      // Two persons are at seat 0 and 4 => expectedly 0 unchanged while person from 4 moved to 2.
      Assert.AreEqual(
        "0:1;1:-1;2:2;",
        SeatAssignmentsToString(
          SeatLayoutUtils.ReassignSeatsKeepingOrderAndMinimizingMovements(
            SeatLayoutIndex.CONFERENCE_LAYOUT_6_SEATS,
            SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS,
            StringToSeatAssignments("0:1;4:2"),
            null
          )
        )
      );

      // Two persons are at seat 0 and 5 => expectedly 0 unchanged while person from 5 moved to 1.
      Assert.AreEqual(
        "0:1;1:2;",
        SeatAssignmentsToString(
          SeatLayoutUtils.ReassignSeatsKeepingOrderAndMinimizingMovements(
            SeatLayoutIndex.CONFERENCE_LAYOUT_6_SEATS,
            SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS,
            StringToSeatAssignments("0:1;5:2"),
            null
          )
        )
      );

      // 4 persons at seats 2,3,4,5 => expectedly 2,3 unchanged while 4 => 0, 5 => 1.
      Assert.AreEqual(
        "0:3;1:4;2:1;3:2;",
        SeatAssignmentsToString(
          SeatLayoutUtils.ReassignSeatsKeepingOrderAndMinimizingMovements(
            SeatLayoutIndex.CONFERENCE_LAYOUT_6_SEATS,
            SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS,
            StringToSeatAssignments("2:1;3:2;4:3;5:4"),
            null
          )
        )
      );

      // 4 persons at seats 0,1,2,3 (full layout) => expectedly unchanged
      Assert.AreEqual(
        "0:1;1:2;2:3;3:4;",
        SeatAssignmentsToString(
          SeatLayoutUtils.ReassignSeatsKeepingOrderAndMinimizingMovements(
            SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS,
            SeatLayoutIndex.CONFERENCE_LAYOUT_4_SEATS,
            StringToSeatAssignments("0:1;1:2;2:3;3:4"),
            null
          )
        )
      );

      // Person at the lectern (player 1) is preserved, while the others moved if needed (7 => 3, 8 => 0).
      Assert.AreEqual(
        "0:4;1:1;2:-1;3:3;4:2;5:-1;6:5;",
        SeatAssignmentsToString(
          SeatLayoutUtils.ReassignSeatsKeepingOrderAndMinimizingMovements(
            SeatLayoutIndex.PRESENTATION_LAYOUT_10_SEATS,
            SeatLayoutIndex.PRESENTATION_LAYOUT_6_SEATS,
            StringToSeatAssignments("1:1;4:2;7:3;8:4;10:5"),
            null
          )
        )
      );
    }

    // Output string format: "<seatIdx>:<playerIdx>;..." ordered by seatIdx and omitting last empty seats.
    private static string SeatAssignmentsToString(Dictionary<int, int> seatAssignments) {
      var seatIdxToPlayerId = new Dictionary<int, int>();
      foreach (var (playerId, seatIdx) in seatAssignments) {
        seatIdxToPlayerId[seatIdx] = playerId;
      }

      StringBuilder strBuilder = new StringBuilder();
      for (var seatIdx = 0; seatIdx <= seatAssignments.Values.Max(); seatIdx++) {
        var playerId = seatIdxToPlayerId.ContainsKey(seatIdx) ? seatIdxToPlayerId[seatIdx] : -1;
        strBuilder.Append($"{seatIdx}:{playerId};");
      }

      return strBuilder.ToString();
    }

    private static Dictionary<int, int> StringToSeatAssignments(string str) {
      var seatAssignments = new Dictionary<int, int>();
      foreach (var seatAssignment in str.Split(';')) {
        var seatIdx = int.Parse(seatAssignment.Split(':')[0]);
        var playerId = int.Parse(seatAssignment.Split(':')[1]);
        if (playerId != -1) {
          seatAssignments[playerId] = seatIdx;
        }
      }

      return seatAssignments;
    }
  }
}
