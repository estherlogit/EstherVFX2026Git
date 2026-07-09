// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Facebook.GraphQLGen.Apps.Workrooms;
using Facebook.SocialVR.Core.Bedrock;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Application;
using Facebook.Workrooms.CanvasService;
using Facebook.Workrooms.CanvasService.Whiteboard;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.Canvas.Service;
using Facebook.Workrooms.Testing;
using Facebook.Xplat.Events;
using Facebook.Xplat.Gatekeeper;
using NSubstitute;
using NUnit.Framework;
using Oculus.Verts;
using Tests.UnitTests.Whiteboard;

namespace Facebook.Workrooms.Tests.UnitTests {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class CanvasDriverTest : IEventSubscriber {
    private CanvasDriver canvasDriver_;
    private IDispatcher dispatcher_;
    private const uint ENTITY_1 = 10001;
    private const uint ENTITY_2 = 10002;
    private const uint COMPONENT_1 = 10001001;
    private const uint COMPONENT_2 = 10001002;
    private const uint COMPONENT_3 = 10001003;
    private const uint DRAWING_1 = 20001001;
    private const uint DRAWING_2 = 20001002;
    private const uint CANVAS_ID = 98000;
    private const ulong ACTION_1 = 5000001;
    private const ulong ACTION_2 = 5000002;

    private bool canvasHistoryFlag_ = false;

    private const int REMOTE_VERTS_SESSION_ID = 3;
    private CanvasPersistence canvasPersistence_;

    [SetUp]
    public void Setup() {
      canvasPersistence_ = Substitute.For<CanvasPersistence>(
        null,
        null,
        null,
        null,
        null
      );
      dispatcher_ = new DispatcherSpyDecorator(new Dispatcher(), Substitute.For<IDispatcher>());

      var workroomsSitevarClient = Substitute.For<WorkroomsSitevarClient>(
        Substitute.For<WorkroomsSitevarQueryModel.ViewerModel.ConfigurationsModel>()
      );
      var gatekeeperClient = new GatekeeperClient(null);
      gatekeeperClient.MockGK(
        Facebook.SocialVR.Apps.Workrooms.Gatekeepers.GatekeeperToNameMap[GK.WORKROOMS_WHITEBOARD_SKIA],
        false
      );

      canvasDriver_ = new CanvasDriver(
        canvasPersistence_,
        dispatcher_,
        Substitute.For<ILogService>(),
        Substitute.For<CanvasConfig>(),
        Substitute.For<IUpdateRunner>(),
        workroomsSitevarClient,
        gatekeeperClient,
        1,
        false
      );
      canvasDriver_.LoadCanvas(CANVAS_ID, 0, 0, "", new List<CanvasDelayedAction>());
      canvasHistoryFlag_ = false;
    }

    [Test]
    public void TestSessionIncrementalVersion() {
      canvasDriver_.LoadCanvas(0, 0, 123, "", new List<CanvasDelayedAction>());
      Assert.AreEqual(123, canvasDriver_.SessionIncrementalVersion);
    }

    [Test]
    public void TestTransactionOnNewEntity1() {
      // 1 start transaction
      // 2 apply actions
      // 3 discard transaction
      canvasDriver_.StartTransaction(ENTITY_1);
      Assert.AreEqual(canvasDriver_.IsEntityInTransaction(ENTITY_1), true);
      ApplyActionHelper();
      canvasDriver_.DiscardTransaction(ENTITY_1);
      Assert.AreEqual(canvasDriver_.IsEntityInTransaction(ENTITY_1), false);
      Assert.AreEqual(canvasDriver_.GetCanvasEntities(CanvasType.MAIN).Count, 0);
      Assert.AreEqual(canvasDriver_.GetCanvasEntities(CanvasType.TEMPORARY_PRIVATE).Count, 0);
    }

    [Test]
    public void TestTransactionOnNewEntity2() {
      // 1 start transaction
      // 2 apply no actions
      // 3 discard transaction
      canvasDriver_.StartTransaction(ENTITY_1);
      Assert.AreEqual(canvasDriver_.IsEntityInTransaction(ENTITY_1), true);
      canvasDriver_.DiscardTransaction(ENTITY_1);
      Assert.AreEqual(canvasDriver_.IsEntityInTransaction(ENTITY_1), false);
      Assert.AreEqual(canvasDriver_.GetCanvasEntities(CanvasType.MAIN).Count, 0);
      Assert.AreEqual(canvasDriver_.GetCanvasEntities(CanvasType.TEMPORARY_PRIVATE).Count, 0);
    }

    [Test]
    public void TestTransactionOnNewEntity3() {
      // 1 start transaction
      // 2 apply actions
      // 3 commit transaction
      canvasDriver_.StartTransaction(ENTITY_1);
      Assert.AreEqual(canvasDriver_.IsEntityInTransaction(ENTITY_1), true);
      ApplyActionHelper();
      canvasDriver_.CommitTransaction(ENTITY_1);
      Assert.AreEqual(canvasDriver_.IsEntityInTransaction(ENTITY_1), false);
      Assert.AreEqual(canvasDriver_.GetCanvasEntities(CanvasType.MAIN).Count, 1);
      Assert.AreEqual(canvasDriver_.GetCanvasEntities(CanvasType.TEMPORARY_PRIVATE).Count, 0);
    }

    [Test]
    public void TestTransactionOnExistingEntity1() {
      // 1 create existing entity
      // 2 start transaction
      // 3 apply actions
      // 4 commit transaction
      canvasDriver_.StartTransaction(ENTITY_1);
      Assert.AreEqual(canvasDriver_.IsEntityInTransaction(ENTITY_1), true);
      ApplyActionHelper();
      canvasDriver_.CommitTransaction(ENTITY_1);
      canvasDriver_.StartTransaction(ENTITY_1);
      Assert.AreEqual(canvasDriver_.IsEntityInTransaction(ENTITY_1), true);
      Assert.AreEqual(canvasDriver_.GetCanvasEntities(CanvasType.TEMPORARY_PRIVATE).Count, 1);
      var drawingPoints = new List<Vector2>() {
        Vector2.One,
        Vector2.UnitX,
        Vector2.UnitY,
        Vector2.Zero,
        new Vector2(1f, 3f),
        new Vector2(3, 7),
        Vector2.One,
      };
      canvasDriver_.ApplyAction(
        new AddDrawingWhiteboardComponentAction(
          EntityID: ENTITY_1,
          ComponentID: COMPONENT_1,
          Drawing: new Drawing(ID: DRAWING_2, Points: drawingPoints.GetRange(0, 3))
        )
      );
      canvasDriver_.CommitTransaction(ENTITY_1);
      Assert.AreEqual(canvasDriver_.GetCanvasEntities(CanvasType.MAIN).Count, 1);
      var whiteboardComponent =
        canvasDriver_.GetCanvasEntities(CanvasType.MAIN)[ENTITY_1].GetComponent<WhiteboardComponent>();
      Assert.AreEqual(whiteboardComponent?.Drawings.Count, 2);
    }

    [Test]
    public void TestTransactionOnExistingEntity2() {
      // 1 create existing entity
      // 2 start transaction
      // 3 apply actions
      // 4 discard transaction
      canvasDriver_.StartTransaction(ENTITY_1);
      Assert.AreEqual(canvasDriver_.IsEntityInTransaction(ENTITY_1), true);
      ApplyActionHelper();
      canvasDriver_.CommitTransaction(ENTITY_1);
      canvasDriver_.StartTransaction(ENTITY_1);
      Assert.AreEqual(canvasDriver_.IsEntityInTransaction(ENTITY_1), true);
      Assert.AreEqual(canvasDriver_.GetCanvasEntities(CanvasType.TEMPORARY_PRIVATE).Count, 1);
      var drawingPoints = new List<Vector2>() {
        Vector2.One,
        Vector2.UnitX,
        Vector2.UnitY,
        Vector2.Zero,
        new Vector2(1f, 3f),
        new Vector2(3, 7),
        Vector2.One,
      };
      canvasDriver_.ApplyAction(
        new AddDrawingWhiteboardComponentAction(
          EntityID: ENTITY_1,
          ComponentID: COMPONENT_1,
          Drawing: new Drawing(ID: DRAWING_2, Points: drawingPoints.GetRange(0, 3))
        )
      );
      canvasDriver_.DiscardTransaction(ENTITY_1);
      Assert.AreEqual(canvasDriver_.GetCanvasEntities(CanvasType.MAIN).Count, 1);
      var whiteboardComponent =
        canvasDriver_.GetCanvasEntities(CanvasType.MAIN)[ENTITY_1].GetComponent<WhiteboardComponent>();
      Assert.AreEqual(whiteboardComponent?.Drawings.Count, 1);
    }

    private void ApplyActionHelper() {
      canvasDriver_.ApplyAction(new AddEntityAction(Entity: new Entity(ID: ENTITY_1)));
      canvasDriver_.ApplyAction(new CreateWhiteboardComponentAction(EntityID: ENTITY_1, ComponentID: COMPONENT_1));
      var drawingPoints = new List<Vector2>() {
        Vector2.One,
        Vector2.UnitX,
        Vector2.UnitY,
        Vector2.Zero,
        new Vector2(1f, 3f),
        new Vector2(3, 7),
        Vector2.One,
      };
      canvasDriver_.ApplyAction(
        new AddDrawingWhiteboardComponentAction(
          EntityID: ENTITY_1,
          ComponentID: COMPONENT_1,
          Drawing: new Drawing(ID: DRAWING_1, Points: drawingPoints.GetRange(0, 3))
        )
      );
    }

    [Test]
    public void TestUndoRedo1() {
      // Transaction can not undo
      canvasDriver_.StartTransaction(ENTITY_1);
      Assert.AreEqual(canvasDriver_.IsEntityInTransaction(ENTITY_1), true);
      ApplyActionHelper();
      canvasDriver_.CommitTransaction(ENTITY_1);
      Assert.AreEqual(canvasDriver_.CanUndo(), false);
    }

    [Test]
    public void TestUndoRedo2() {
      ApplyActionHelper();
      Assert.AreEqual(canvasDriver_.CanUndo(), true);
      var whiteboardComponent =
        canvasDriver_.GetCanvasEntities(CanvasType.MAIN)[ENTITY_1].GetComponent<WhiteboardComponent>();
      Assert.AreEqual(whiteboardComponent?.Drawings.Count, 1);
      canvasDriver_.Undo();
      Assert.AreEqual(whiteboardComponent?.Drawings.Count, 0);
      Assert.AreEqual(canvasDriver_.CanRedo(), true);
      canvasDriver_.Redo();
      Assert.AreEqual(whiteboardComponent?.Drawings.Count, 1);
    }

    [Test]
    public void TestCanvasHistory() {
      dispatcher_.Subscribe<CanvasHistoryChanged>(this, OnHistoryChanged);
      ApplyActionHelper();
      Assert.AreEqual(canvasHistoryFlag_, true);
    }

    [Test]
    public void TestCanvasHistory2() {
      // switch canvas can trigger history changed event
      dispatcher_.Subscribe<CanvasHistoryChanged>(this, OnHistoryChanged);
      canvasDriver_.LoadCanvas(0, 0, 0, "", new List<CanvasDelayedAction>());
      Assert.AreEqual(canvasHistoryFlag_, true);
    }

    private void OnHistoryChanged() {
      canvasHistoryFlag_ = true;
    }

    [Test]
    public void TestOnApplyActionHandler1() {
      // test OnApplyLocalActionHandler can be invoked
      var localApplied = 0;
      canvasDriver_.OnApplyLocalActionHandler += (in IAction action, ulong actionID, CanvasOperationResult result, WhiteboardRealtimeClientCAPI.ActionType actionType) => {
        localApplied += 1;
      };
      ApplyActionHelper();
      Assert.AreEqual(localApplied, 3);
    }

    [Test]
    public void TestOnApplyActionHandler2() {
      // test OnApplyNetworkedActionHandler can be invoked
      var remoteApplied = 0;
      canvasDriver_.OnApplyNetworkedActionHandler +=
        (in IAction action, ulong actionID, CanvasOperationResult result, WhiteboardRealtimeClientCAPI.ActionType actionType) => { remoteApplied += 1; };
      canvasDriver_.ApplyRemoteActionToMainCanvasModel(CANVAS_ID, 0, new AddEntityAction(new Entity(0)), false);
      Assert.AreEqual(remoteApplied, 1);
    }

    [Test]
    public void TestLoadingStateChanged() {
      // test LoadingStateChanged can be invoked
      var loading = false;
      canvasDriver_.LoadingState.LoadingStateChanged += (loadingState, tmp) => { loading = tmp; };
      canvasDriver_.LoadCanvas(0, 0, 0, "", new List<CanvasDelayedAction>());
      Assert.AreEqual(loading, false);
    }

    [Test]
    public void TestLoadingDelayedAction() {
      var delayedActions = new List<CanvasDelayedAction>() {
        new CanvasDelayedAction(
          CANVAS_ID,
          ACTION_1,
          REMOTE_VERTS_SESSION_ID,
          new AddEntityAction(
            Entity: new Entity(
              ID: ENTITY_1,
              Components: new Dictionary<ulong, IComponent>() {
                { COMPONENT_1, new TransformComponent(ID: COMPONENT_1, position: Vector2.One, size: Vector2.UnitX, 0) },
              }
            )
          ),
          null
        ),
        new CanvasDelayedAction(
          CANVAS_ID,
          ACTION_2,
          REMOTE_VERTS_SESSION_ID,
          new CreateImageEntityAction(
            EntityID: ENTITY_2,
            Transform: new TransformComponent(ID: COMPONENT_2, position: Vector2.One, size: Vector2.UnitX, 0),
            Texture: new TextureComponent(ID: COMPONENT_3, TextureFbid: 0)
          ),
          null
        ),
      };
      canvasDriver_.LoadCanvas(CANVAS_ID, 0, 0, "", delayedActions);
      Assert.AreEqual(canvasDriver_.GetCanvasEntities(CanvasType.MAIN).Count, 2);
    }

    [Test]
    public void TestLoadingDelayedActionWithWrongCanvasID() {
      var delayedActions = new List<CanvasDelayedAction>() {
        new CanvasDelayedAction(
          CANVAS_ID,
          ACTION_1,
          REMOTE_VERTS_SESSION_ID,
          new AddEntityAction(
            Entity: new Entity(
              ID: ENTITY_1,
              Components: new Dictionary<ulong, IComponent>() {
                { COMPONENT_1, new TransformComponent(ID: COMPONENT_1, position: Vector2.One, size: Vector2.UnitX, 0) },
              }
            )
          ),
          null
        ),
        new CanvasDelayedAction(
          CANVAS_ID + 1,
          ACTION_2,
          REMOTE_VERTS_SESSION_ID,
          new CreateImageEntityAction(
            EntityID: ENTITY_2,
            Transform: new TransformComponent(ID: COMPONENT_2, position: Vector2.One, size: Vector2.UnitX, 0),
            Texture: new TextureComponent(ID: COMPONENT_3, TextureFbid: 0)
          ),
          null
        ),
      };
      canvasDriver_.LoadCanvas(CANVAS_ID, 0, 0, "", delayedActions);
      Assert.AreEqual(canvasDriver_.GetCanvasEntities(CanvasType.MAIN).Count, 1);
    }

    [Test]
    public void TestLoadingSerializedDelayedAction() {
      // test delta actions can be applied as expect
      var delayedActions = new List<CanvasDelayedAction>() {
        new CanvasDelayedAction(
          CANVAS_ID,
          ACTION_1,
          REMOTE_VERTS_SESSION_ID,
          new AddEntityAction(
            Entity: new Entity(
              ID: ENTITY_1,
              Components: new Dictionary<ulong, IComponent>() {
                { COMPONENT_1, new TransformComponent(ID: COMPONENT_1, position: Vector2.One, size: Vector2.UnitX, 0) },
              }
            )
          ),
          null
        ),
        new CanvasDelayedAction(
          CANVAS_ID,
          ACTION_2,
          REMOTE_VERTS_SESSION_ID,
          new CreateImageEntityAction(
            EntityID: ENTITY_2,
            Transform: new TransformComponent(ID: COMPONENT_2, position: Vector2.One, size: Vector2.UnitX, 0),
            Texture: new TextureComponent(ID: COMPONENT_3, TextureFbid: 0)
          ),
          null
        ),
      };
      var serialized = canvasPersistence_.SerializeDelayedActions(delayedActions);
      canvasDriver_.LoadCanvas(CANVAS_ID, 0, 0, serialized, new List<CanvasDelayedAction>());
      Assert.AreEqual(canvasDriver_.GetCanvasEntities(CanvasType.MAIN).Count, 2);
    }

    [Test]
    public void TestLoadingSerializedDelayedAction2() {
      // if delta actions overlap with buffered actions
      var delayedActions = new List<CanvasDelayedAction>() {
        new CanvasDelayedAction(
          CANVAS_ID,
          ACTION_1,
          REMOTE_VERTS_SESSION_ID,
          new AddEntityAction(
            Entity: new Entity(
              ID: ENTITY_1,
              Components: new Dictionary<ulong, IComponent>() {
                { COMPONENT_1, new TransformComponent(ID: COMPONENT_1, position: Vector2.One, size: Vector2.UnitX, 0) },
              }
            )
          ),
          null
        ),
        new CanvasDelayedAction(
          CANVAS_ID,
          ACTION_2,
          REMOTE_VERTS_SESSION_ID,
          new CreateImageEntityAction(
            EntityID: ENTITY_2,
            Transform: new TransformComponent(ID: COMPONENT_2, position: Vector2.One, size: Vector2.UnitX, 0),
            Texture: new TextureComponent(ID: COMPONENT_3, TextureFbid: 0)
          ),
          null
        ),
      };
      // overlap action
      var delayedActions2 = new List<CanvasDelayedAction>() {
        new CanvasDelayedAction(
          CANVAS_ID,
          ACTION_1,
          REMOTE_VERTS_SESSION_ID,
          new AddEntityAction(
            Entity: new Entity(
              ID: ENTITY_1,
              Components: new Dictionary<ulong, IComponent>() {
                { COMPONENT_1, new TransformComponent(ID: COMPONENT_1, position: Vector2.One, size: Vector2.UnitX, 0) },
              }
            )
          ),
          null
        ),
        new CanvasDelayedAction(
          CANVAS_ID,
          ACTION_2 + 1,
          REMOTE_VERTS_SESSION_ID,
          new CreateImageEntityAction(
            EntityID: ENTITY_2 + 1,
            Transform: new TransformComponent(ID: COMPONENT_2, position: Vector2.One, size: Vector2.UnitX, 0),
            Texture: new TextureComponent(ID: COMPONENT_3, TextureFbid: 0)
          ),
          null
        ),
      };
      var serialized = canvasPersistence_.SerializeDelayedActions(delayedActions);
      canvasDriver_.LoadCanvas(CANVAS_ID, 0, 0, serialized, delayedActions2);
      Assert.AreEqual(canvasDriver_.GetCanvasEntities(CanvasType.MAIN).Count, 3);
    }

    public void Dispose() {
      dispatcher_.Unsubscribe(this);
    }
  }
}
