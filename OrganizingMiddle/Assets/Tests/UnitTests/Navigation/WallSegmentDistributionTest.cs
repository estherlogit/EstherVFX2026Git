using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Navigation;
using NUnit.Framework;

namespace Facebook.Workrooms.Tests.UnitTests.Navigation {
  [OnCall(OnCallName.workrooms_core)]
  public class WallSegmentDistributionTest {

    [Test]
    public void FindNextWallSegmentTest() {
      Assert.AreEqual(WallSegmentServiceIfMaster.FindNextWallSegment(0, 0, 0), WallSegment.MIDDLE);
      Assert.AreEqual(WallSegmentServiceIfMaster.FindNextWallSegment(0, 1, 0), WallSegment.MIDDLE);
      Assert.AreEqual(WallSegmentServiceIfMaster.FindNextWallSegment(0, 2, 0), WallSegment.LEFT);
      Assert.AreEqual(WallSegmentServiceIfMaster.FindNextWallSegment(1, 2, 0), WallSegment.LEFT);
      Assert.AreEqual(WallSegmentServiceIfMaster.FindNextWallSegment(2, 2, 0), WallSegment.RIGHT);
      Assert.AreEqual(WallSegmentServiceIfMaster.FindNextWallSegment(2, 2, 1), WallSegment.RIGHT);
      Assert.AreEqual(WallSegmentServiceIfMaster.FindNextWallSegment(2, 2, 2), WallSegment.MIDDLE);
      Assert.AreEqual(WallSegmentServiceIfMaster.FindNextWallSegment(2, 3, 2), WallSegment.LEFT);
      Assert.AreEqual(WallSegmentServiceIfMaster.FindNextWallSegment(3, 3, 2), WallSegment.RIGHT);
      Assert.AreEqual(WallSegmentServiceIfMaster.FindNextWallSegment(1, 0, 0), WallSegment.LEFT);
      Assert.AreEqual(WallSegmentServiceIfMaster.FindNextWallSegment(0, 0, 1), WallSegment.RIGHT);
      Assert.AreEqual(WallSegmentServiceIfMaster.FindNextWallSegment(2, 0, 0), WallSegment.MIDDLE);
      Assert.AreEqual(WallSegmentServiceIfMaster.FindNextWallSegment(0, 0, 2), WallSegment.MIDDLE);
      Assert.AreEqual(WallSegmentServiceIfMaster.FindNextWallSegment(2, 1, 0), WallSegment.MIDDLE);
      Assert.AreEqual(WallSegmentServiceIfMaster.FindNextWallSegment(2, 2, 0), WallSegment.RIGHT);
      Assert.AreEqual(WallSegmentServiceIfMaster.FindNextWallSegment(2, 2, 4), WallSegment.MIDDLE);
    }
  }
}
