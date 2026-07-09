// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.Surfaces;
using NUnit.Framework;
using UnityEngine;

namespace Facebook.Workrooms.Tests.UnitTests.Whiteboard {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class CanvasHoverInputHandlerTest {
    private StickyNotesConfig stickyNotesConfig_;

    private readonly Vector3 defaultPosition_ = Vector3.zero;
    private readonly Vector3 defaultScale_ = 100 * Vector3.one;
    private readonly float buttonSize_ = 10;
    private readonly float buttonOffset_ = 20;
    private readonly float hoverPadding_ = 5;

    [SetUp]
    public void Setup() {
      stickyNotesConfig_ = ScriptableObject.CreateInstance<StickyNotesConfig>();
      stickyNotesConfig_.ContextUIDeskEditButtonSize = buttonSize_;
      stickyNotesConfig_.ContextUIWallEditButtonSize = buttonSize_;
      stickyNotesConfig_.ContextUIDeskYOffset = buttonOffset_;
      stickyNotesConfig_.ContextUIWallYEditButtonOffset = buttonOffset_;
      stickyNotesConfig_.ContextPaddingOnHover = hoverPadding_;
    }

    [TearDown]
    public void TearDown() {
      Object.DestroyImmediate(stickyNotesConfig_);
    }

    [Test]
    public void CheckNotSelected() {
      InputHandlerArgsGenerator argsGenerator = new();
      argsGenerator.IsSelected = false;

      CanvasHoverInputHandler wallHoverInputHandler = new(WorkroomsLocation.WALL, stickyNotesConfig_);
      Assert.IsFalse(wallHoverInputHandler.IsHit(argsGenerator.Generate()), "Wall: Not selected");

      CanvasHoverInputHandler deskHoverInputHandler = new(WorkroomsLocation.DESK, stickyNotesConfig_);
      Assert.IsFalse(deskHoverInputHandler.IsHit(argsGenerator.Generate()),"Desk: Not selected");
    }

    [Test]
    public void CheckHitsNoZoom() {
      InputHandlerArgsGenerator argsGenerator = new();
      argsGenerator.ElementPosition = defaultPosition_;
      argsGenerator.ElementScale = defaultScale_;
      argsGenerator.IsSelected = true;
      argsGenerator.ZoomFactor = 1;

      float horizontalCenter = defaultPosition_.x + defaultScale_.x * 0.5f;
      float verticalCenter = defaultPosition_.y - buttonOffset_;
      float leftBorder = horizontalCenter - buttonSize_ * 0.5f;
      float rightBorder = horizontalCenter + buttonSize_ * 0.5f;
      float topBorder = defaultPosition_.y + defaultScale_.y; // Input holder hit box expends up to the to of the element
      float bottomBorder = verticalCenter - buttonSize_ * 0.5f;
      TestBorders(
        WorkroomsLocation.WALL,
        false,
        argsGenerator,
        horizontalCenter,
        verticalCenter,
        leftBorder,
        rightBorder,
        topBorder,
        bottomBorder);
      TestBorders(
        WorkroomsLocation.DESK,
        false,
        argsGenerator,
        horizontalCenter,
        verticalCenter,
        leftBorder,
        rightBorder,
        topBorder,
        bottomBorder);

      leftBorder -= hoverPadding_;
      rightBorder += hoverPadding_;
      bottomBorder -= hoverPadding_;
      // topBorder doesn't need the extra padding as it's already reaching the top the parent
      TestBorders(WorkroomsLocation.WALL,
        true,
        argsGenerator,
        horizontalCenter,
        verticalCenter,
        leftBorder,
        rightBorder,
        topBorder,
        bottomBorder);
      TestBorders(WorkroomsLocation.DESK,
        true,
        argsGenerator,
        horizontalCenter,
        verticalCenter,
        leftBorder,
        rightBorder,
        topBorder,
        bottomBorder);
    }

    [Test]
    public void CheckHitsZoom() {
      InputHandlerArgsGenerator argsGenerator = new();
      argsGenerator.ElementPosition = defaultPosition_;
      argsGenerator.ElementScale = defaultScale_;
      argsGenerator.IsSelected = true;
      argsGenerator.ZoomFactor = 2.5f;

      float zoomedButtonSize = buttonSize_ / argsGenerator.ZoomFactor;
      float zoomedButtonOffset = buttonOffset_ / argsGenerator.ZoomFactor;

      float horizontalCenter = defaultPosition_.x + defaultScale_.x * 0.5f;
      float verticalCenter = defaultPosition_.y - zoomedButtonOffset;
      float leftBorder = horizontalCenter - zoomedButtonSize * 0.5f;
      float rightBorder = horizontalCenter + zoomedButtonSize * 0.5f;
      float topBorder = defaultPosition_.y + defaultScale_.y; // Input holder hit box expends up to the to of the element
      float bottomBorder = verticalCenter - zoomedButtonSize * 0.5f;
      TestBorders(WorkroomsLocation.DESK,
        false,
        argsGenerator,
        horizontalCenter,
        verticalCenter,
        leftBorder,
        rightBorder,
        topBorder,
        bottomBorder);

      leftBorder -= hoverPadding_;
      rightBorder += hoverPadding_;
      bottomBorder -= hoverPadding_;
      // topBorder doesn't need the extra padding as it's already reaching the top the parent
      TestBorders(
        WorkroomsLocation.DESK,
        true,
        argsGenerator,
        horizontalCenter,
        verticalCenter,
        leftBorder,
        rightBorder,
        topBorder,
        bottomBorder);
    }

    private void TestBorders(
      WorkroomsLocation location,
      bool triggerHover,
      InputHandlerArgsGenerator argsGenerator,
      float horizontalCenter,
      float verticalCenter,
      float leftBorder,
      float rightBorder,
      float topBorder,
      float bottomBorder
    ) {
      CanvasHoverInputHandler inputHandler = new(location, stickyNotesConfig_);
      if (triggerHover) {
        inputHandler.HoverEnter(Vector3.zero);
      }

      string paddingLog = triggerHover ? "Padding" : "No padding";

      // Inside the hit box
      // Left
      argsGenerator.InputPosition = new Vector3(leftBorder + 1, verticalCenter, 0);
      Assert.IsTrue(inputHandler.IsHit(argsGenerator.Generate()), "Left, In, " + location + ", " + paddingLog + ": " + argsGenerator.Log());

      // Right
      argsGenerator.InputPosition = new Vector3(rightBorder - 1, verticalCenter, 0);
      Assert.IsTrue(inputHandler.IsHit(argsGenerator.Generate()), "Right, In, " + location + ", " + paddingLog + ": " + argsGenerator.Log());

      // Top
      argsGenerator.InputPosition = new Vector3(horizontalCenter, topBorder - 1, 0);
      Assert.IsTrue(inputHandler.IsHit(argsGenerator.Generate()), "Top, In, " + location + ", " + paddingLog + ": " + argsGenerator.Log());

      // Bottom
      argsGenerator.InputPosition = new Vector3(horizontalCenter, bottomBorder + 1, 0);
      Assert.IsTrue(inputHandler.IsHit(argsGenerator.Generate()), "Bottom, In, " + location + ", " + paddingLog + ": " + argsGenerator.Log());


      // Outside the hitbox
      // Left
      argsGenerator.InputPosition = new Vector3(leftBorder - 1, verticalCenter, 0);
      Assert.IsFalse(inputHandler.IsHit(argsGenerator.Generate()), "Left, Off, " + location + ", " + paddingLog + ": " + argsGenerator.Log());

      // Right
      argsGenerator.InputPosition = new Vector3(rightBorder + 1, verticalCenter, 0);
      Assert.IsFalse(inputHandler.IsHit(argsGenerator.Generate()), "Right, Off, " + location + ", " + paddingLog + ": " + argsGenerator.Log());

      // Top
      argsGenerator.InputPosition = new Vector3(horizontalCenter, topBorder + 1, 0);
      Assert.IsFalse(inputHandler.IsHit(argsGenerator.Generate()), "Top, Off, " + location + ", " + paddingLog + ": " + argsGenerator.Log());

      // Bottom
      argsGenerator.InputPosition = new Vector3(horizontalCenter, bottomBorder - 1, 0);
      Assert.IsFalse(inputHandler.IsHit(argsGenerator.Generate()), "Bottom, Off, " + location + ", " + paddingLog + ": " + argsGenerator.Log());
    }

    struct InputHandlerArgsGenerator {
      public Vector3 InputPosition { get; set; }
      public Vector3 ElementPosition { get; set; }
      public Vector3 ElementScale { get; set; }
      public bool IsSelected { get; set; }
      public float ZoomFactor { get; set; }

      public InputHandlerArgs Generate() {
        return new InputHandlerArgs(
          InputPosition,
          ElementPosition,
          ElementScale,
          IsSelected,
          ZoomFactor);
      }

      public string Log() {
        return "[InputPosition: "
               + InputPosition
               + ", ElementPosition: "
               + ElementPosition
               + ", ElementScale: "
               + ElementScale
               + ", IsSelected: "
               + IsSelected
               + ", ZoomFactor: "
               + ZoomFactor
               + "]";
      }
    }
  }
}
