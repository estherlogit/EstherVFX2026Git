// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using Facebook.SocialVR.Core.Bedrock;
using Facebook.SocialVR.Core.Inputs;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.Canvas.SetupSequence;
using Facebook.Workrooms.InputDevice;
using Facebook.Workrooms.Surfaces;
using Facebook.Workrooms.Testing;
using Facebook.Workrooms.WhiteboardTool;
using Facebook.Xplat.Events;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using static Facebook.Workrooms.Canvas.CanvasInputStateMachine;

namespace Facebook.Workrooms.Tests.UnitTests.Whiteboard {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class ToolAutoSelectionControllerTest {
    private const WhiteboardToolStateTypes DEFAULT_NON_DRAWING_TOOL = WhiteboardToolStateTypes.SELECT;

    private ILocalPlayerSurfaceAnchorController localPlayerSurfaceAnchorController_;
    private ICanvasSetupController deskCanvasSetupController_;
    private IWhiteboardToolsStateService whiteboardToolsStateService_;
    private IWhiteboardToolsService whiteboardToolsService_;
    private IInputMethodService inputMethodService_;
    private ToolAutoSelectionController autoSelectionController_;
    private DispatcherSpyDecorator dispatcher_;
    private IDevicePlatform devicePlatform_;

    [SetUp]
    public void Setup() {
      whiteboardToolsStateService_ = Substitute.For<IWhiteboardToolsStateService>();
      whiteboardToolsService_ = Substitute.For<IWhiteboardToolsService>();
      localPlayerSurfaceAnchorController_ = Substitute.For<ILocalPlayerSurfaceAnchorController>();
      var canvasSetupService = new MockCanvasSetupService(Substitute.For<ICanvasSetupController>());
      deskCanvasSetupController_ = canvasSetupService.DeskSetupController;
      deskCanvasSetupController_.ShouldHideWhiteboard.Returns(false);
      inputMethodService_ = Substitute.For<IInputMethodService>();
      dispatcher_ = new DispatcherSpyDecorator(new Dispatcher(), Substitute.For<IDispatcher>());
      devicePlatform_ = Substitute.For<IDevicePlatform>();
      devicePlatform_.IsVREnabled().Returns(true);
      ReinitToolAutoSelectionController();
    }

    [Test]
    public void DefaultToolSelectedWhenMovingToWall() {
      MoveToLocation(WorkroomsLocation.DESK, WorkroomsLocation.WALL);
      TestDefaultToolEventReceived();
    }

    [Test]
    public void PanningToolSelectedWhenMovingToDeskWithoutCalibrationComplete() {
      deskCanvasSetupController_.IsComplete.Returns(false);
      MoveToLocation(WorkroomsLocation.WALL, WorkroomsLocation.DESK);
      TestToolEventReceived(WhiteboardToolStateTypes.SELECT);
    }

    [Test]
    public void DefaultToolSelectedWhenMovingToDeskAndCalibrationComplete() {
      deskCanvasSetupController_.IsComplete.Returns(true);
      MoveToLocation(WorkroomsLocation.WALL, WorkroomsLocation.DESK);
      TestDefaultToolEventReceived();
    }

    [Test]
    public void DontResetToolWhenMovingAroundSameLocation() {
      // eg, moving between segments at wall
      deskCanvasSetupController_.IsComplete.Returns(true);
      MoveToLocation(WorkroomsLocation.WALL, WorkroomsLocation.WALL);
      TestDefaultToolEventNotReceived();
    }

    [Test]
    public void PreviousToolSelectedAfterDeskCanvasSetupComplete() {
      MoveToLocation(WorkroomsLocation.WALL, WorkroomsLocation.DESK);
      ReinitToolAutoSelectionController();
      var previousToolState = new ToolsState() {
        ActiveTool = WhiteboardToolStateTypes.DRAWING,
        ColorString = "green"
      };
      whiteboardToolsStateService_.ToolsState.Returns(previousToolState);

      // Start the canvas setup
      deskCanvasSetupController_.IsComplete.Returns(false);
      deskCanvasSetupController_.IsSetupActive.Returns(true);
      TriggerCanvasSetupStepChangedEvent();

      // complete the setup
      deskCanvasSetupController_.IsComplete.Returns(true);
      deskCanvasSetupController_.IsSetupActive.Returns(false);
      TriggerCanvasSetupStepChangedEvent();

      TestToolEventReceived(previousToolState.ActiveTool, previousToolState.ColorString);
    }

    [Test]
    public void DefaultToolSelectedAfterDeskCanvasSetupCompleteIfPreviouslySelectedToolWasPanning() {
      MoveToLocation(WorkroomsLocation.WALL, WorkroomsLocation.DESK);
      ReinitToolAutoSelectionController();
      whiteboardToolsStateService_.ToolsState.Returns(
        new ToolsState() {
          ActiveTool = WhiteboardToolStateTypes.SELECT,
        }
      );

      // Start the canvas setup
      deskCanvasSetupController_.IsComplete.Returns(false);
      deskCanvasSetupController_.IsSetupActive.Returns(true);
      TriggerCanvasSetupStepChangedEvent();

      // complete the setup
      deskCanvasSetupController_.IsComplete.Returns(true);
      deskCanvasSetupController_.IsSetupActive.Returns(false);
      TriggerCanvasSetupStepChangedEvent();

      TestDefaultToolEventReceived();
    }

    [Test]
    public void PanningSelectedIfDeskCanvasSetupCancelled() {
      MoveToLocation(WorkroomsLocation.WALL, WorkroomsLocation.DESK);
      ReinitToolAutoSelectionController();
      whiteboardToolsStateService_.ToolsState.Returns(
        new ToolsState() {
          ActiveTool = WhiteboardToolStateTypes.DRAWING,
          ColorString = "green"
        }
      );

      // Start the canvas setup
      deskCanvasSetupController_.IsComplete.Returns(false);
      deskCanvasSetupController_.IsSetupActive.Returns(true);
      TriggerCanvasSetupStepChangedEvent();

      // cancel the setup
      deskCanvasSetupController_.IsComplete.Returns(false);
      deskCanvasSetupController_.IsSetupActive.Returns(false);
      TriggerCanvasSetupStepChangedEvent();

      TestToolEventReceived(WhiteboardToolStateTypes.SELECT);
    }

    [Test]
    public void PenSelectedIfLocalPlayerAnchorChangedWhileDeskCanvasSetupActive() {
      MoveToLocation(WorkroomsLocation.WALL, WorkroomsLocation.DESK);
      ReinitToolAutoSelectionController();

      // Start the canvas setup
      deskCanvasSetupController_.IsComplete.Returns(false);
      deskCanvasSetupController_.IsSetupActive.Returns(true);
      TriggerCanvasSetupStepChangedEvent();

      // Clear the tool = drawing event which has just been received
      whiteboardToolsService_.ClearReceivedCalls();

      // tool gets changed to non-drawing for any other reason, eg, moving to wall and back
      whiteboardToolsStateService_.ToolsState.Returns(new ToolsState {ActiveTool = WhiteboardToolStateTypes.SELECT});

      // Test that triggering the step progress again switches the tool back to drawing
      TriggerCanvasSetupStepChangedEvent();
      TestToolEventReceived(WhiteboardToolStateTypes.DRAWING, ToolAutoSelectionController.DEFAULT_COLOR);
    }

    [Test]
    public void DefaultToolSelectedWhenDeskCanvasSetupStarted() {
      ReinitToolAutoSelectionController();
      deskCanvasSetupController_.IsComplete.Returns(false);
      deskCanvasSetupController_.IsSetupActive.Returns(true);
      TriggerCanvasSetupStepChangedEvent();
      TestDefaultToolEventReceived();
    }

    [Test]
    public void UnselectUnsupportedToolTypeWhenSwitchingToHands() {
      MoveToLocation(WorkroomsLocation.DESK, WorkroomsLocation.DESK);

      ReinitToolAutoSelectionController();
      deskCanvasSetupController_.IsComplete.Returns(false);
      deskCanvasSetupController_.IsSetupActive.Returns(false);

      // Image mode is supported with hands, so don't switch
      whiteboardToolsStateService_.ToolsState.Returns(new ToolsState {ActiveTool = WhiteboardToolStateTypes.IMAGE});
      TriggerInputMethodChangedEvent(InputMethod.HANDS);
      TestNonDrawingToolEventNotReceived();

      // Drawing mode is not supported with hands, so switch
      whiteboardToolsStateService_.ToolsState.Returns(new ToolsState {ActiveTool = WhiteboardToolStateTypes.DRAWING});
      TriggerInputMethodChangedEvent(InputMethod.HANDS);
      TestNonDrawingToolEventReceived();
    }

    [Test]
    public void DontSwitchToolTypeWhenUsingHandsAtWall() {
      MoveToLocation(WorkroomsLocation.DESK, WorkroomsLocation.WALL);

      ReinitToolAutoSelectionController();
      deskCanvasSetupController_.IsComplete.Returns(false);
      deskCanvasSetupController_.IsSetupActive.Returns(false);

      // Image mode is supported with hands, so don't switch
      whiteboardToolsStateService_.ToolsState.Returns(new ToolsState {ActiveTool = WhiteboardToolStateTypes.DRAWING});
      TriggerInputMethodChangedEvent(InputMethod.HANDS);
      TestNonDrawingToolEventNotReceived();
    }

    [Test]
    public void ChangeFromSelectToolWhenEnteringAnnotationInputStateWithController() {
      MoveToLocation(WorkroomsLocation.WALL, WorkroomsLocation.DESK);

      ReinitToolAutoSelectionController();
      deskCanvasSetupController_.IsComplete.Returns(true);

      // Tool doesn't switch immediately when transition starts
      whiteboardToolsStateService_.ToolsState.Returns(new ToolsState {ActiveTool = WhiteboardToolStateTypes.SELECT});
      TriggerCanvasInputStateChangedEvent(activeState: State.DEFAULT, targetState: State.ANNOTATE);
      TestToolEventNotReceived(WhiteboardToolStateTypes.DRAWING, ToolAutoSelectionController.DEFAULT_COLOR);

      // Tool switches to drawing when annotation input state transition complete
      TriggerCanvasInputStateChangedEvent(activeState: State.ANNOTATE, targetState: State.ANNOTATE);
      TestToolEventReceived(WhiteboardToolStateTypes.DRAWING, ToolAutoSelectionController.DEFAULT_COLOR);
    }

    [Test]
    public void ChangeFromSelectToolWhenEnteringAnnotationInputStateWithHands() {
      MoveToLocation(WorkroomsLocation.WALL, WorkroomsLocation.DESK);

      ReinitToolAutoSelectionController();
      deskCanvasSetupController_.IsComplete.Returns(true);
      inputMethodService_.IsUsingHands().Returns(true);

      // Tool switches to default non drawing after the input state transition completes, not drawing
      TriggerCanvasInputStateChangedEvent(activeState: State.DEFAULT, targetState: State.ANNOTATE);
      TriggerCanvasInputStateChangedEvent(activeState: State.ANNOTATE, targetState: State.ANNOTATE);
      TestToolEventNotReceived(WhiteboardToolStateTypes.DRAWING, ToolAutoSelectionController.DEFAULT_COLOR);
      TestNonDrawingToolEventReceived();
    }

    [Test]
    public void ToolDoesNotChangeBetweenNonSelectStateWhenEnteringAnnotationInputState() {
      MoveToLocation(WorkroomsLocation.WALL, WorkroomsLocation.DESK);
      ReinitToolAutoSelectionController();
      var previousToolState = new ToolsState() {
        ActiveTool = WhiteboardToolStateTypes.ERASER,
      };
      whiteboardToolsStateService_.ToolsState.Returns(previousToolState);

      // Tool doesn't change during transition, or after transition
      TriggerCanvasInputStateChangedEvent(activeState: State.DEFAULT, targetState: State.ANNOTATE);
      TriggerCanvasInputStateChangedEvent(activeState: State.ANNOTATE, targetState: State.ANNOTATE);
      TestToolEventNotReceived(WhiteboardToolStateTypes.DRAWING, ToolAutoSelectionController.DEFAULT_COLOR);
    }

    [Test]
    public void ControllerToHandsToControllersInAnnotationModeDoesNotLeaveUserInSelectMode() {
      MoveToLocation(WorkroomsLocation.WALL, WorkroomsLocation.DESK);
      whiteboardToolsStateService_.ToolsState.Returns(new ToolsState {ActiveTool = WhiteboardToolStateTypes.DRAWING});

      // Normally switching to HANDS auto switches to non-drawing tool
      TriggerInputMethodChangedEvent(InputMethod.HANDS);
      TestNonDrawingToolEventReceived();

      // Reset controller state, enter annotation mode
      whiteboardToolsStateService_.ToolsState.Returns(new ToolsState {ActiveTool = WhiteboardToolStateTypes.DRAWING});
      TriggerInputMethodChangedEvent(InputMethod.CONTROLLER);
      whiteboardToolsService_.ClearReceivedCalls();
      TriggerCanvasInputStateChangedEvent(State.ANNOTATE, State.ANNOTATE);

      // Now switching to HANDS keeps drawing tool active
      TriggerInputMethodChangedEvent(InputMethod.HANDS);
      TestNonDrawingToolEventNotReceived();

      TriggerInputMethodChangedEvent(InputMethod.CONTROLLER);
      TestNonDrawingToolEventNotReceived();
    }

    private void MoveToLocation(WorkroomsLocation fromLoc, WorkroomsLocation newLoc) {
      localPlayerSurfaceAnchorController_.LocalPlayerAnchorLocation.Returns(newLoc);
      dispatcher_.Dispatch(
        new LocalPlayerAnchorLocationChangedEvent() {
          OldAnchorLocation = fromLoc,
          NewAnchorLocation = newLoc
        }
      );
    }

    private void TestDefaultToolEventReceived() {
      TestToolEventReceived(ToolAutoSelectionController.DEFAULT_TOOL, ToolAutoSelectionController.DEFAULT_COLOR);
    }

    private void TestDefaultToolEventNotReceived() {
      TestToolEventNotReceived(ToolAutoSelectionController.DEFAULT_TOOL, ToolAutoSelectionController.DEFAULT_COLOR);
    }

    private void TestNonDrawingToolEventReceived() {
      TestToolEventReceived(DEFAULT_NON_DRAWING_TOOL, null);
    }

    private void TestNonDrawingToolEventNotReceived() {
      TestToolEventNotReceived(DEFAULT_NON_DRAWING_TOOL, null);
    }

    private void TestToolEventReceived(WhiteboardToolStateTypes type, string color = null) {
      whiteboardToolsService_.Received()
        .SetActiveTool(
          Arg.Is<WhiteboardToolStateTypes>(e => e == type),
          Arg.Is<string>(e => e == color),
          Arg.Is<int>(e => e == 0),
          Arg.Any<WorkroomsLocation>(),
          Arg.Is<WhiteboardToolsService.SetActiveToolSource>(
            e => e == WhiteboardToolsService.SetActiveToolSource.C_SHARP
          )
        );
    }

    private void TestToolEventNotReceived(WhiteboardToolStateTypes type, string color = null) {
      whiteboardToolsService_.DidNotReceive()
        .SetActiveTool(
          Arg.Is<WhiteboardToolStateTypes>(e => e == type),
          Arg.Is<string>(e => e == color),
          Arg.Is<int>(e => e == 0),
          Arg.Is<WorkroomsLocation>(e => e == WorkroomsLocation.DESK),
          Arg.Is<WhiteboardToolsService.SetActiveToolSource>(
            e => e == WhiteboardToolsService.SetActiveToolSource.C_SHARP
          )
        );
    }

    private void TriggerCanvasInputStateChangedEvent(State activeState, State targetState) {
      dispatcher_.Dispatch(
        new CanvasInputStateController.StateUpdatedEvent().Init(
          new CanvasInputStateTransitionStatus(activeState, targetState)
        )
      );
    }

    private void TriggerCanvasSetupStepChangedEvent() {
      dispatcher_.Dispatch(new CanvasSetupSequenceStepChangedEvent());
    }

    private void TriggerInputMethodChangedEvent(InputMethod inputMethod) {
      dispatcher_.Dispatch(
        new InputMethodChangedEvent(new HandedInputMethod(Handedness.LEFT, inputMethod, false), InputMethod.CONTROLLER)
      );
    }

    private void ReinitToolAutoSelectionController() {
      if (autoSelectionController_ != null) {
        autoSelectionController_.Dispose();
      }

      autoSelectionController_ = new ToolAutoSelectionController(
        localPlayerSurfaceAnchorController_,
        deskCanvasSetupController_,
        whiteboardToolsService_,
        whiteboardToolsStateService_,
        dispatcher_,
        inputMethodService_,
        DEFAULT_NON_DRAWING_TOOL,
        devicePlatform_
      );
    }
  }
}
