// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections.Generic;
using System.Numerics;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.CanvasService;
using Facebook.Workrooms.CanvasService.Whiteboard;
using Facebook.Workrooms.React.Modules;
using Facebook.Workrooms.Whiteboard;
using NUnit.Framework;

namespace Facebook.Workrooms.Tests.UnitTests.Whiteboard {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class CanvasWhiteboardEventsTest {

    private const uint CANVAS_ID = 10000;
    private const int REMOTE_VERTS_SESSION_ID = 3;
    private const ulong ACTION_ID = 20323;
    private const ulong DRAWING_ID = 70001;
    private const uint COLOR = 34544;
    private const uint THICKNESS = 17;
    private const int REACTVR_WHITEBOARD_ID = 11111;

    [Test]
    public void WhiteboardAddDrawingRPCToEvent() {
      var whiteboardEvent = new NetworkedReactVRReceiveAddEntityActionEvent(
        REACTVR_WHITEBOARD_ID,
        new AddDrawingWhiteboardComponentAction(
          EntityID: WhiteboardReactVRModule.MAIN_WHITEBOARD_RESERVED_ENTITY_ID,
          ComponentID: WhiteboardReactVRModule.MAIN_WHITEBOARD_RESERVED_COMPONENT_ID,
          Drawing: new Drawing(ID: DRAWING_ID, Points: null, Color: COLOR, Thickness: THICKNESS)
        )
      );

      Assert.AreEqual(
        (DRAWING_ID, COLOR, THICKNESS),
        (whiteboardEvent.EntityID, whiteboardEvent.Color, whiteboardEvent.Thickness)
      );
    }

    [Test]
    public void WhiteboardAddDrawingPointsRPCToEvent() {
      var whiteboardEvent = new NetworkedReactVRReceiveAddPointsActionEvent(
        REACTVR_WHITEBOARD_ID,
        new AddDrawingPointsWhiteboardComponentAction(
          EntityID: WhiteboardReactVRModule.MAIN_WHITEBOARD_RESERVED_ENTITY_ID,
          ComponentID: WhiteboardReactVRModule.MAIN_WHITEBOARD_RESERVED_COMPONENT_ID,
          DrawingID: DRAWING_ID,
          IsInProgress: false,
          Points: new List<Vector2>() {
            Vector2.UnitX,
            Vector2.UnitY
          }
        )
      );

      Assert.AreEqual(DRAWING_ID, whiteboardEvent.EntityID);

      Assert.AreEqual(
        new List<float>() {
          Vector2.UnitX.X,
          Vector2.UnitX.Y,
          Vector2.UnitY.X,
          Vector2.UnitY.Y
        },
        whiteboardEvent.FlattenedPoints
      );
    }

    [Test]
    public void WhiteboardDeleteDrawingRPCToEvent() {
      var whiteboardEvent = new NetworkedReactVRReceiveDeleteEntitiesActionEvent(
        REACTVR_WHITEBOARD_ID,
        new DeleteDrawingWhiteboardComponentAction(
          EntityID: WhiteboardReactVRModule.MAIN_WHITEBOARD_RESERVED_ENTITY_ID,
          ComponentID: WhiteboardReactVRModule.MAIN_WHITEBOARD_RESERVED_COMPONENT_ID,
          DrawingID: DRAWING_ID
        )
      );

      Assert.AreEqual(DRAWING_ID, whiteboardEvent.EntityID);
    }
  }
}
