// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using Facebook.SocialVR.Core.OnCall;
using Facebook.SocialVR.Modules.MRDStreaming;
using Facebook.Workrooms.DesktopMirror;
using NUnit.Framework;
using System.Collections.Generic;

namespace Facebook.Workrooms.Tests.UnitTests {
  [OnCall(OnCallName.workrooms_remote_desktop)]
  public class ScreenLayoutTests {
    // These variable names encode the layout they contain:
    // - ph for placeholder
    // - Px for physical monitor with id x
    // - Vx for virtual monitor with id x
    readonly ScreenLayout phphP1 = new(new []{ScreenLayout.Placeholder, ScreenLayout.Placeholder, Physical(1)});
    readonly ScreenLayout phP1ph = new(new []{ScreenLayout.Placeholder, Physical(1), ScreenLayout.Placeholder});
    readonly ScreenLayout phP1V2 = new(new []{ScreenLayout.Placeholder, Physical(1), Virtual(2)});
    readonly ScreenLayout P3P1P2 = new(new []{Physical(3), Physical(1), Physical(2)});
    readonly ScreenLayout P1P2P3 = new(new []{Physical(1), Physical(2), Physical(3)});
    readonly ScreenLayout V1P1P2 = new(new []{Virtual(1), Physical(1), Physical(2)});
    readonly ScreenLayout P3V1P1 = new(new []{Physical(3), Virtual(1), Physical(1)});
    readonly ScreenLayout P1P2V1V2 = new(new []{Physical(1), Physical(2), Virtual(1), Virtual(2)});

    static ScreenLayout.Screen Physical(int id) {
      return new ScreenLayout.Screen(id, ScreenLayout.Type.Physical);
    }

    static ScreenLayout.Screen Virtual(int id) {
      return new ScreenLayout.Screen(id, ScreenLayout.Type.Virtual);
    }

    List<VRScreen> ToVrScreens(ScreenLayout layout) {
      List<VRScreen> vrScreens = new();
      foreach (ScreenLayout.Screen screen in layout) {
        if (screen.IsMonitor) {
          VideoStreamSourceType type = VideoStreamSourceType.VirtualDisplay;
          if (screen.Type == ScreenLayout.Type.Physical) {
            type = VideoStreamSourceType.PhysicalDisplay;
          }
          vrScreens.Add(new VRScreen(screen.Id, type));
        }
      }
      return vrScreens;
    }

    [SetUp]
    public void Setup() {
      TestContext.AddFormatter<VRScreen>(screen => ToString(screen as VRScreen));
    }

    public static string ToString(VRScreen screen) {
      return screen == null ? "null" : $"{screen.Type} {screen.ID}";
    }

    public static string Message(List<VRScreen> from, List<VRScreen> to) {
      return $"From: {string.Join(", ", from.ConvertAll(ToString))}\n" +
        $"To:   {string.Join(", ", to.ConvertAll(ToString))}";
    }

    void PerformRearrangement(ScreenLayout from, ScreenLayout to, int[] expectedOrder) {
      // The starting point for rearragning is always a list of VRScreens. However, it is more convenient
      // here to work only with ScreenLayouts for consistency. So we convert a "from" layout to a list of VRScreens.
      List<VRScreen> vrScreens = ToVrScreens(from);
      // Perform the actual rearranging
      List<VRScreen> rearranged = to.Rearrange(vrScreens);
      // This message will help in troubleshooting failing asserts
      string message = Message(vrScreens, rearranged);

      Assert.AreEqual(expectedOrder.Length, rearranged.Count, message);
      for (int index = 0; index < expectedOrder.Length; index++) {
        Assert.AreEqual(vrScreens[expectedOrder[index]], rearranged[index], message);
      }
    }

    [Test]
    public void SingleMonitorNeedsNoRearrangingMove() {
      // Moving the monitor to the right does not change the order as there is only 1
      PerformRearrangement(phP1ph, phphP1, new int[]{0});
    }

    [Test]
    public void SingleMonitorNeedsNoRearrangingMultiple() {
      // In a target layout with multiple monitors there still is only 1 input
      PerformRearrangement(phP1ph, V1P1P2, new int[]{0});
    }

    [Test]
    public void RearrangeThreePhysicalA() {
      PerformRearrangement(P3P1P2, P1P2P3, new int[]{1, 2, 0});
    }

    [Test]
    public void RearrangeThreePhysicalB() {
      PerformRearrangement(P1P2P3, P3P1P2, new int[]{2, 0, 1});
    }

    [Test]
    public void RearrangeThreeWithOneVirtualA() {
      PerformRearrangement(V1P1P2, P3V1P1, new int[]{2, 0, 1});
    }

    [Test]
    public void RearrangeThreeWithOneVirtualB() {
      PerformRearrangement(P3V1P1, V1P1P2, new int[]{1, 2, 0});
    }

    [Test]
    public void RearrangeThreeFromOneVirtualToAllPhysicalA() {
      PerformRearrangement(V1P1P2, P1P2P3, new int[]{1, 2, 0});
    }

    [Test]
    public void RearrangeThreeFromOneVirtualToAllPhysicalB() {
      PerformRearrangement(P3V1P1, P1P2P3, new int[]{2, 1, 0});
    }

    [Test]
    public void RearrangeThreeFromAllPhysicalToOneVirtualA() {
      PerformRearrangement(P1P2P3, V1P1P2, new int[]{2, 0, 1});
    }

    [Test]
    public void RearrangeThreeFromAllPhysicalToOneVirtualB() {
      PerformRearrangement(P1P2P3, P3V1P1, new int[]{2, 1, 0});
    }

    [Test]
    public void RearrangeMoreMonitorsThanLayoutHas4to3() {
      PerformRearrangement(P1P2V1V2, P3V1P1, new int[]{1, 2, 0, 3});
    }

    [Test]
    public void RearrangeMoreMonitorsThanLayoutHas3to2() {
      PerformRearrangement(P3V1P1, phP1V2, new int[]{1, 2, 0});
    }

    [Test]
    public void RearrangeFewerMonitorsThanLayoutHas3to4() {
      PerformRearrangement(P3V1P1, P1P2V1V2, new int[]{2, 0, 1});
    }

    [Test]
    public void RearrangeFewerMonitorsThanLayoutHas2to3() {
      // The placeholder doesn't get a VRScreen, so this is from P1V2 to P3V1P1
      PerformRearrangement(phP1V2, P3V1P1, new int[]{1, 0});
    }
  }
}
