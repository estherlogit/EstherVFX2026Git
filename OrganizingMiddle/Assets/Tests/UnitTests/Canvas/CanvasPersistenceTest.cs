// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.OnCall;
using NUnit.Framework;
using Facebook.Workrooms.CanvasService;
using Facebook.Workrooms.CanvasService.Whiteboard;
using Facebook.Workrooms.Canvas;
using Facebook.Xplat.GraphClients;
using Newtonsoft.Json;
using UnityEngine.UIElements;

namespace Facebook.Workrooms.Tests.UnitTests {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class CanvasPersistenceTest {

    private const uint CANVAS_ID = 98000;

    private const uint SEGMENT_1 = 101;
    private const uint SEGMENT_2 = 102;

    private const ulong ENTITY_1 = 10001;
    private const ulong ENTITY_2 = 10002;
    private const ulong ENTITY_3 = 10003;
    private const ulong ENTITY_4 = 10004;

    private const ulong COMPONENT_1 = 10001001;
    private const ulong COMPONENT_2 = 10001002;
    private const ulong COMPONENT_3 = 10001003;
    private const ulong COMPONENT_4 = 10001004;
    private const ulong COMPONENT_5 = 10001005;
    private const ulong COMPONENT_6 = 10001006;

    private const ulong DRAWING_1 = 20001001;
    private const ulong DRAWING_2 = 20001002;
    private const ulong DRAWING_3 = 20001003;

    private const ulong TEXTURE_FBID = 123123;

    private const ulong ACTION_1 = 5000001;
    private const ulong ACTION_2 = 5000002;

    private const int REMOTE_VERTS_SESSION_ID = 3;

    private static readonly IReadOnlyList<(string Title, IReadOnlyDictionary<ulong, Entity> Entities)> stateTestCases_ =
      new List<(string Title, IReadOnlyDictionary<ulong, Entity> Entities)> {
        (Title: "Empty entities", Entities: new Dictionary<ulong, Entity>() { }),
        (Title: "Single entity with no components",
          Entities: new Dictionary<ulong, Entity>() {
            {ENTITY_1, new Entity(ID: ENTITY_1)},
          }),
        (Title: "Multiple entities, same segment",
          Entities: new Dictionary<ulong, Entity>() {
            {ENTITY_1, new Entity(ID: ENTITY_1)},
            {ENTITY_2, new Entity(ID: ENTITY_3)},
            {ENTITY_3, new Entity(ID: ENTITY_4)},
          }),
        (Title: "Multiple entities, multiple segments",
          Entities: new Dictionary<ulong, Entity>() {
            {ENTITY_1, new Entity(ID: ENTITY_1)},
            {ENTITY_2, new Entity(ID: ENTITY_3)},
            {ENTITY_3, new Entity(ID: ENTITY_4)},
          }),
        (Title: "Transform component",
          Entities: new Dictionary<ulong, Entity>() {
            {
              ENTITY_1,
              new Entity(
                ID: ENTITY_1,
                Components: new Dictionary<ulong, IComponent>() {
                  {COMPONENT_1, new TransformComponent(ID: COMPONENT_1, Vector2.UnitY, Vector2.One, 0)}
                }
              )
            },
          }),
        (Title: "Texture component",
          Entities: new Dictionary<ulong, Entity>() {
            {
              ENTITY_1,
              new Entity(
                ID: ENTITY_1,
                Components: new Dictionary<ulong, IComponent>() {
                  {COMPONENT_1, new TextureComponent(ID: COMPONENT_1, TextureFbid: TEXTURE_FBID)}
                }
              )
            },
          }),
        (Title: "Whiteboard component with no drawings",
          Entities: new Dictionary<ulong, Entity>() {
            {
              ENTITY_1,
              new Entity(
                ID: ENTITY_1,
                Components: new Dictionary<ulong, IComponent>() {
                  {COMPONENT_1, new WhiteboardComponent(ID: COMPONENT_1)}
                }
              )
            },
          }),
        (Title: "Whiteboard component with drawings",
          Entities: new Dictionary<ulong, Entity>() {
            {
              ENTITY_1,
              new Entity(
                ID: ENTITY_1,
                Components: new Dictionary<ulong, IComponent>() {
                  {
                    COMPONENT_1,
                    new WhiteboardComponent(
                      ID: COMPONENT_1,
                      Drawings: new Dictionary<ulong, Drawing>() {
                        {
                          DRAWING_1,
                          new Drawing(ID: DRAWING_1, Points: new Vector2[] {Vector2.One, Vector2.Zero, Vector2.UnitX})
                        }, {
                          DRAWING_2,
                          new Drawing(ID: DRAWING_2, Points: new Vector2[] {Vector2.UnitX, Vector2.Zero, Vector2.UnitX})
                        }, {
                          DRAWING_3,
                          new Drawing(ID: DRAWING_3, Points: new Vector2[] {Vector2.One, Vector2.Zero, Vector2.UnitY})
                        },
                      }
                    )
                  }
                }
              )
            },
          }),
        (Title: "Single entity, multiple components",
          Entities: new Dictionary<ulong, Entity>() {
            {
              ENTITY_1,
              new Entity(
                ID: ENTITY_1,
                Components: new Dictionary<ulong, IComponent>() {
                  {COMPONENT_1, new TransformComponent(ID: COMPONENT_1, Vector2.UnitY, Vector2.One, 0)},
                  {COMPONENT_2, new TextureComponent(ID: COMPONENT_2, TextureFbid: TEXTURE_FBID)},
                }
              )
            },
          }),
        (Title: "Multiple entities, multiple components",
          Entities: new Dictionary<ulong, Entity>() {
            {
              ENTITY_1,
              new Entity(
                ID: ENTITY_1,
                Components: new Dictionary<ulong, IComponent>() {
                  {COMPONENT_1, new TransformComponent(ID: COMPONENT_1, Vector2.UnitY, Vector2.One, 0)},
                  {COMPONENT_2, new TextureComponent(ID: COMPONENT_2, TextureFbid: TEXTURE_FBID)},
                }
              )
            }, {
              ENTITY_2,
              new Entity(
                ID: ENTITY_2,
                Components: new Dictionary<ulong, IComponent>() {
                  {COMPONENT_4, new TransformComponent(ID: COMPONENT_4, Vector2.One, Vector2.Zero, 0)},
                  {COMPONENT_6, new TextureComponent(ID: COMPONENT_6, TextureFbid: TEXTURE_FBID)}
                }
              )
            },
          }),
      };

    private static readonly string serializedRTCSAction_ = @"{
      'ActionType': 'AddDrawingPointsWhiteboardComponentAction',
      'DrawingID': '619890234469200003',
      'ComponentID': '2147483646',
      'EntityID': '2147483642',
      'IsInProgress': true,
      'Points': [[0,0]]
    }";

    private static readonly IAction deserializedRTCSAction_ = new AddDrawingWhiteboardComponentAction(
      2147483642,
      2147483646,
      new Drawing(619890234469200003)
    );

    private CanvasPersistence canvasPersistence_;

    [SetUp]
    public void Setup() {
      canvasPersistence_ = new CanvasPersistence(
        null,
        null,
        null,
        null,
        null
      );
    }

    [Test]
    public void TestSerializeAndDeserializeCanvasState() {
      foreach (var (Title, ExpectedState) in stateTestCases_) {
        var canvas = new CanvasService.CanvasService(ExpectedState);

        var serialized = canvasPersistence_.SerializeCanvasService(canvas);
        var deserializedCanvas = canvasPersistence_.DeserializeCanvasService(serialized);

        Assert.AreEqual(ExpectedState.Keys, canvas.Entities.Keys, "{0}: Canvas entity keys didn't match", Title);
        AssertAllEntitiesAreEqual(ExpectedState, deserializedCanvas.Entities);
      }
    }

    [Test]
    public void TestSerializeAndDeserializeDelayedActions() {
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
            Texture: new TextureComponent(ID: COMPONENT_3, TextureFbid: TEXTURE_FBID)
          ),
          null
        ),
      };

      var serialized = canvasPersistence_.SerializeDelayedActions(delayedActions);
      var deserialized = canvasPersistence_.DeserializeDelayedActions(serialized);

      Assert.AreEqual(2, deserialized.Count, "Expecting there to be two actions in the deserialized list");
      Assert.AreEqual(
        typeof(AddEntityAction),
        deserialized[0].Action.GetType(),
        "Expected the first action type to be AddEntityAction"
      );
      Assert.AreEqual(
        typeof(CreateImageEntityAction),
        deserialized[1].Action.GetType(),
        "Expected the second action type to be CreateImageEntityAction"
      );
      AssertEntitiesAreEqual(
        ((AddEntityAction)delayedActions[0].Action).Entity,
        ((AddEntityAction)deserialized[0].Action).Entity
      );

      Assert.AreEqual(
        ((CreateImageEntityAction)delayedActions[1].Action).EntityID,
        ((CreateImageEntityAction)deserialized[1].Action).EntityID,
        "Entity IDs should be equal"
      );
      Assert.AreEqual(
        ((CreateImageEntityAction)delayedActions[1].Action).Transform.Position,
        ((CreateImageEntityAction)deserialized[1].Action).Transform.Position,
        "Transform positions should be equal"
      );
      Assert.AreEqual(
        ((CreateImageEntityAction)delayedActions[1].Action).Transform.Size,
        ((CreateImageEntityAction)deserialized[1].Action).Transform.Size,
        "Transform sizes should be equal"
      );
      Assert.AreEqual(
        ((CreateImageEntityAction)delayedActions[1].Action).Transform.DepthLayer,
        ((CreateImageEntityAction)deserialized[1].Action).Transform.DepthLayer,
        "Transform depth layer should be equal"
      );
      Assert.AreEqual(
        ((CreateImageEntityAction)delayedActions[1].Action).Texture.TextureFbid,
        ((CreateImageEntityAction)deserialized[1].Action).Texture.TextureFbid,
        "Texture FBIDs should be equal"
      );
    }

    private void AssertEntitiesAreEqual(Entity entity, Entity other) {
      Assert.AreEqual(entity.ID, other.ID, "Entity IDs should be equal");
      Assert.AreEqual(entity.Components.Count, other.Components.Count, "Entity components count should be equal");
      foreach (var componentKVP in entity.Components) {
        Assert.IsTrue(
          other.Components.ContainsKey(componentKVP.Key),
          "Other entity must contain the same component keys"
        );

        var component = componentKVP.Value;
        var otherComponent = other.Components[componentKVP.Key];
        Assert.AreEqual(component.ID, otherComponent.ID, "Component IDs should be equal");
        Assert.AreEqual(component.GetType(), otherComponent.GetType(), "Component types should be equal");

        switch (component) {
          case WhiteboardComponent whiteboard:
            var otherWhiteboard = (WhiteboardComponent)otherComponent;
            Assert.AreEqual(
              whiteboard.Drawings.Count,
              otherWhiteboard.Drawings.Count,
              "Whiteboard drawings count should be equal"
            );

            foreach (var drawingKVP in whiteboard.Drawings) {
              Assert.IsTrue(
                otherWhiteboard.Drawings.ContainsKey(drawingKVP.Key),
                "Other whiteboard must contain the same drawings keys"
              );

              var drawing = drawingKVP.Value;
              var otherDrawingPoints = otherWhiteboard.Drawings[drawingKVP.Key].Points.ToList();
              Assert.AreEqual(drawing.Points.Count, otherDrawingPoints.Count, "Drawing points count should be equal");

              foreach (var point in drawing.Points) {
                Assert.Contains(point, otherDrawingPoints, "Other drawing must have the same points");
              }
            }

            break;

          case TransformComponent transformComponent:
            var otherTransformComponent = (TransformComponent)otherComponent;
            Assert.AreEqual(
              transformComponent.Position,
              otherTransformComponent.Position,
              "Transform positions should be equal"
            );
            Assert.AreEqual(transformComponent.Size, otherTransformComponent.Size, "Sizes should be equal");
            break;

          case TextureComponent textureComponent:
            var otherTextureComponent = (TextureComponent)otherComponent;
            Assert.AreEqual(
              textureComponent.TextureFbid,
              otherTextureComponent.TextureFbid,
              "Texture FBIDs should be equal"
            );
            break;

          default:
            throw new ArgumentException($"Unhandled component type: {component.GetType()}");
        }
      }
    }

    [Test]
    public void TestDeserializeRTCSAction() {
      var action = CanvasPersistence.DeserializeRTCSAction(serializedRTCSAction_);
      Assert.IsTrue(action is AddDrawingPointsWhiteboardComponentAction);
    }

    [Test]
    public void TestSerializeAndDeserializeRTCSAction() {
      var deserializedAction = CanvasPersistence.SerializeRTCSAction(deserializedRTCSAction_);
      var action = CanvasPersistence.DeserializeRTCSAction(deserializedAction);
      Assert.IsTrue(action is AddDrawingWhiteboardComponentAction);
    }

    [Test]
    public void TestSerializeAndDeserializeDrawingWithTooManyPoints() {
      var points = Enumerable.Repeat(Vector2.One, (int) Drawing.MAX_DRAWING_POINTS + 7).ToList();
      var delayedActions = new List<CanvasDelayedAction>() {
        new CanvasDelayedAction(
          CANVAS_ID,
          ACTION_1,
          REMOTE_VERTS_SESSION_ID,
          new AddEntityAction(
            Entity: new Entity(
              ID: ENTITY_1,
              Components: new Dictionary<ulong, IComponent>() {
                {
                  COMPONENT_1,
                  new WhiteboardComponent(
                    ID: COMPONENT_1,
                    Drawings: new Dictionary<ulong, Drawing>() {
                      {
                        DRAWING_1,
                        new Drawing(ID: DRAWING_1, Points: points)
                      },
                    }
                  )
                }
              }
            )
          ),
          null
        ),
      };

      var serialized = canvasPersistence_.SerializeDelayedActions(delayedActions);
      var deserialized = canvasPersistence_.DeserializeDelayedActions(serialized);

      Assert.AreEqual(1, deserialized.Count, "Expecting there to be two actions in the deserialized list");
      Assert.AreEqual(
        typeof(AddEntityAction),
        deserialized[0].Action.GetType(),
        "Expected the first action type to be AddEntityAction"
      );
      Assert.AreEqual(
        true,
        ((AddEntityAction) deserialized[0].Action).Entity.Components.TryGetValue(
          COMPONENT_1,
          out var whiteboardComponent
        ),
        "Expected the entity to have a component"
      );
      Assert.AreEqual(
        typeof(WhiteboardComponent),
        whiteboardComponent.GetType(),
        "Expected the component to be of whiteboard component"
      );
      Assert.AreEqual(
        true,
        ((WhiteboardComponent)whiteboardComponent).Drawings.TryGetValue(DRAWING_1, out var drawing),
        "Expected the whiteboard component to have a drawing with the expected ID"
      );
      Assert.AreEqual(
        Drawing.MAX_DRAWING_POINTS,
        drawing.Points.Count,
        "Expected the drawing points to be trimmed down"
      );
    }

    private void AssertAllEntitiesAreEqual(
      IReadOnlyDictionary<ulong, Entity> entities,
      IReadOnlyDictionary<ulong, Entity> otherEntities
    ) {
      Assert.AreEqual(entities.Keys, otherEntities.Keys, "Entity keys didn't match");

      foreach (var kvp in entities) {
        AssertEntitiesAreEqual(kvp.Value, otherEntities[kvp.Key]);
      }
    }
  }
}
