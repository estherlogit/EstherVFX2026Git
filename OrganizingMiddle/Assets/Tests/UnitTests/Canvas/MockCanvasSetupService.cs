// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Canvas.SetupSequence;
using Facebook.Workrooms.Surfaces;
using static Facebook.Workrooms.Surfaces.WorkroomsLocation;

namespace Facebook.Workrooms.Tests.UnitTests.Whiteboard {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class MockCanvasSetupService : ICanvasSetupService {
    public ICanvasSetupController DeskSetupController { get; }
    public ICanvasSetupController WallSetupController { get; }

    public MockCanvasSetupService(ICanvasSetupController deskSetupController) {
      DeskSetupController = deskSetupController;
      WallSetupController = new DeskCanvasSetupController.DummyCanvasSetupController();
    }

    public ICanvasSetupController GetSetupController(WorkroomsLocation location) {
      return location == DESK ? DeskSetupController : WallSetupController;
    }
  }
}
