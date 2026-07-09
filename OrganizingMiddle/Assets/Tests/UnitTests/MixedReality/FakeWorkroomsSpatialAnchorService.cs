// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Facebook.Workrooms.MixedReality.Anchors;

namespace Tests.UnitTests.MixedReality {
  public class FakeWorkroomsSpatialAnchorService : IWorkroomsSpatialAnchorService {

    private Dictionary<Guid, OVRPose> anchors = new();

    public Task<AnchorOperationResult> LoadAnchorByGuid(Guid guid, SpatialAnchorStorageLocation location, CancellationToken token, double timeout = 0d) {
      throw new NotImplementedException();
    }

    public Task<AnchorOperationResult> CreateSpatialAnchor(OVRPose worldPose) {
      throw new NotImplementedException();
    }

    public Task<AnchorOperationResult> SaveSpatialAnchor(Guid guid, SpatialAnchorStorageLocation location) {
      throw new NotImplementedException();
    }

    public void DestorySpatialAnchor(Guid guid) { }

    public Task<AnchorOperationResult> EraseSpatialAnchor(Guid guid) {
      throw new NotImplementedException();
    }

    public Task<AnchorOperationResult> ShareSpatialAnchor(Guid guid, List<string> userIds) {
      throw new NotImplementedException();
    }

    public OVRPose GetSpatialAnchorPose(Guid guid) {
      return anchors[guid];
    }

    public bool IsAnchorSharable(Guid guid) {
      throw new NotImplementedException();
    }

  }
}
