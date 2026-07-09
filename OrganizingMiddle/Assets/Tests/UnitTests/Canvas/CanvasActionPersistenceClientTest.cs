// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections.Generic;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Threading;
using Facebook.Workrooms.CanvasService;
using Facebook.Workrooms.Canvas;
using NSubstitute;
using NUnit.Framework;
using System.Numerics;
using System.Threading.Tasks;
using Facebook.SocialVR.Core.OnCall;
using NSubstitute.Core;
using Facebook.Xplat.Threading;

namespace Tests.UnitTests.Whiteboard {

  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class CanvasActionPersistenceClientTest {
    private CanvasActionPersistenceClient canvasActionPersistenceClient_;
    private CanvasPersistence mockPersistence_;
    private const ulong ENTITY_1 = 10001;
    private const ulong COMPONENT_1 = 10001001;



  [SetUp]
    public void Setup() {
      mockPersistence_ = Substitute.For<CanvasPersistence>(null,null,null,null,null);
      canvasActionPersistenceClient_ = new CanvasActionPersistenceClient(
        mockPersistence_,
        new UnityLogService(),
        new ThreadManager()
      );
    }

    [Test]
    public void TestCanvasActionPersistenceClientFlow() {
      mockPersistence_.SerializeCanvasActions(Arg.Any<IReadOnlyList<ValueTuple<Int64, IAction>>>())
        .Returns(
          x => {
            var y = (IReadOnlyList<ValueTuple<Int64, IAction>>) x[0];
            if (y.Count == 1) {
              return y.Count.ToString();
            }

            throw new Exception();
          }
        );
      canvasActionPersistenceClient_.SendCanvasAction(
        new AddEntityAction(
          Entity: new Entity(
            ID: ENTITY_1,
            Components: new Dictionary<ulong, IComponent>() {
              {COMPONENT_1, new TransformComponent(ID: COMPONENT_1, position: Vector2.One, size: Vector2.UnitX, 0)},
            }
          )
        ),
        9,
        0
      );

      canvasActionPersistenceClient_.Flush().WrapErrors();
    }

    [Test]
    public void TestCanvasActionMerge() {
      canvasActionPersistenceClient_.SendCanvasAction(
        new AddDrawingPointsWhiteboardComponentAction(
          EntityID: ENTITY_1,
          ComponentID: COMPONENT_1,
          DrawingID: 1000,
          IsInProgress: false,
          Points: new List<Vector2>() {
            Vector2.UnitX,
            Vector2.UnitY
          }
        ),
        9,
        0
      );

      // merge
      canvasActionPersistenceClient_.SendCanvasAction(
        new AddDrawingPointsWhiteboardComponentAction(
          EntityID: ENTITY_1,
          ComponentID: COMPONENT_1,
          DrawingID: 1000,
          IsInProgress: false,
          Points: new List<Vector2>() {
            Vector2.UnitX,
            Vector2.UnitY
          }
        ),
        9,
        0
      );

      // not merge
      canvasActionPersistenceClient_.SendCanvasAction(
        new AddDrawingPointsWhiteboardComponentAction(
          EntityID: ENTITY_1,
          ComponentID: COMPONENT_1 + 1,
          DrawingID: 1000,
          IsInProgress: false,
          Points: new List<Vector2>() {
            Vector2.UnitX,
            Vector2.UnitY
          }
        ),
        9,
        0
      );

      mockPersistence_.SerializeCanvasActions(Arg.Any<IReadOnlyList<ValueTuple<Int64, IAction>>>())
        .Returns(
          x => {
            var y = (IReadOnlyList<ValueTuple<Int64, IAction>>) x[0];
            if (y.Count == 2) {
              return y.Count.ToString();
            }

            throw new Exception();
          }
        );

      canvasActionPersistenceClient_.Flush().WrapErrors();
    }

    [TearDown]
    public void TearDown() {
      canvasActionPersistenceClient_.Dispose();
    }
  }
}
