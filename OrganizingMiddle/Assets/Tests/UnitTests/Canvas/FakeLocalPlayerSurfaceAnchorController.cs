// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Surfaces;
using Facebook.Workrooms.Navigation;
using UnityEngine;

namespace Facebook.Workrooms.Tests.UnitTests.Whiteboard {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  class FakeLocalPlayerSurfaceAnchorController : ILocalPlayerSurfaceAnchorController {
    public MixedRealityAnchorType LocalPlayerAnchorType { get; }
    public WorkroomsLocation LocalPlayerAnchorLocation { get; set; }
    public SurfaceAnchorBoundsState LocalPlayerOutOfBoundsState { get; }
    public MixedRealityAnchorType? DestinationAnchorType { get; }
    public WallSegment LocalPlayerAnchorSegmentPosition { get; }
    public WallSegment PreviousSegmentPosition { get; }
    public WorkroomsLocation? DestinationLocation { get; }
    public float LocalPlayerOutOfBoundsLevel { get; }

    public void SetLocalPlayerAnchorType(MixedRealityAnchorType anchorType, WallSegment? segmentPosition) { }

    public void SetLocalPlayerMidairWhiteboardAnchor(WallSegment segmentPosition) { }

    public void SetLocalPlayerPhysicalWhiteboardAnchor(WallSegment segmentPosition) { }

    public void SetLocalPlayerDeskAnchor() { }

    public void SetDestinationSurface(WorkroomsLocation? location, WallSegment? segmentPosition) { }

    public MixedRealitySurfaceAnchor GetRootForLocation(WorkroomsLocation location) {
      return null;
    }

    public MixedRealityAnchorType? GetAnchorTypeForCurrentOrDestinationLocation(WorkroomsLocation location) {
      return LocalPlayerAnchorType;
    }

    public void SetDestinationAnchorType(MixedRealityAnchorType? anchorType) { }

    public SurfaceAnchorBoundsState GetBoundsStateForLocation(
      WorkroomsLocation anchor,
      bool onlyConsiderLocalOrDestinationAnchor = true
    ) {
      return SurfaceAnchorBoundsState.IN_BOUNDS;
    }
  }
}
