// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Reflection;
using Facebook.SocialVR.Core.Analytics;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.OnCall;
using Facebook.SocialVR.Core.Player;
using Facebook.SocialVR.Core.Utils;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.Canvas.SetupSequence;
using Facebook.Workrooms.InputDevice;
using Facebook.Workrooms.Passthrough;
using Facebook.Workrooms.Scene;
using Facebook.Workrooms.Surfaces;
using Facebook.Workrooms.Testing;
using Facebook.Workrooms.WhiteboardTool;
using Facebook.Xplat.Analytics;
using Facebook.Xplat.Events;
using Facebook.Xplat.Gatekeeper;
using Facebook.Xplat.GraphClients;
using Facebook.XRTechMR.SurfaceRefinement;
using NSubstitute;
using NUnit.Framework;

namespace Facebook.Workrooms.Tests.UnitTests {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class CanvasSetupSequenceControllerTest {
    private CanvasSetupSequenceController canvasSetupSequenceController_;
    private IInputMethodService inputMethodService_;
    private IDispatcher dispatcher_;

    [SetUp]
    public void Setup() {
      dispatcher_ = new DispatcherSpyDecorator(new Dispatcher(), Substitute.For<IDispatcher>());
      inputMethodService_ = Substitute.For<IInputMethodService>();
      var gkClient = new GatekeeperClient(null);
      var ocGKClient = new OCGatekeeperClient(null);
      canvasSetupSequenceController_ = new CanvasSetupSequenceController(
        dispatcher_,
        WorkroomsLocation.DESK,
        visualRefs: null,
        surfaceFinder: null,
        Substitute.For<ICanvasInstance>(),
        Substitute.For<IDeskWidgetController>(),
        inputMethodService_,
        Substitute.For<IAnalytics>(),
        Substitute.For<IQPLLogger>(),
        Substitute.For<IVideoClipReferences>(),
        Substitute.For<WhiteboardCalibrationVisualsConfig>(),
        Substitute.For<ICanvasConfig>(),
        Substitute.For<IMixedRealityAnchorService>(),
        Substitute.For<IPassthroughService>(),
        Substitute.For<IPlayerDriver>(),
        Substitute.For<ILog>(),
        gkClient,
        ocGKClient
      );
    }

    [TearDown]
    public void Cleanup() {
      canvasSetupSequenceController_.Dispose();
    }

    private void SetActiveStepTransition(StepTransition transition) {
      var activeStep = canvasSetupSequenceController_.ActiveStep;
      var property = activeStep.GetType().GetProperty("StepTransition", BindingFlags.Public | BindingFlags.Instance);
      property!.SetValue(activeStep, transition);
    }

    [Test]
    public void TestFirstTimeRanierCalibrationFlow() {
      canvasSetupSequenceController_
        .ResetDeskCanvasSetupCompleted(); // Resets the PlayerPrefs flags for both stylus tips used and returning user flows
      canvasSetupSequenceController_.StartSequence(CanvasSetupStartReason.SETTINGS_MENU);
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.SETUP_INTRO,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      SetActiveStepTransition(StepTransition.NEXT);
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.CLEAR_YOUR_DESK,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      SetActiveStepTransition(StepTransition.NEXT);
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.TAP_DESK,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      SetActiveStepTransition(StepTransition.NEXT);
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.DRAW_A_CIRCLE,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      SetActiveStepTransition(StepTransition.NEXT);
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.DRAW_A_CIRCLE_COMPLETE,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
    }

    [Test]
    public void TestFirstTimeOlympusWithoutStylusCalibrationFlow() {
      inputMethodService_.IsUsingOlympusControllers().Returns(true);
      canvasSetupSequenceController_.ResetDeskCanvasSetupCompleted();
      canvasSetupSequenceController_.StartSequence(CanvasSetupStartReason.SETTINGS_MENU);
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.SETUP_INTRO,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      SetActiveStepTransition(StepTransition.NEXT);
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.CLEAR_YOUR_DESK,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      SetActiveStepTransition(StepTransition.NEXT);
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.ATTACH_STYLUS_TIP,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      SetActiveStepTransition(StepTransition.NEXT);
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.TAP_DESK,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      SetActiveStepTransition(StepTransition.NEXT);
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.DRAW_A_CIRCLE,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      SetActiveStepTransition(StepTransition.NEXT);
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.DRAW_A_CIRCLE_COMPLETE,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
    }

    [Test]
    public void TestReturningRanierCalibrationFlow() {
      canvasSetupSequenceController_.EndSequence(); // Ending the sequence first sets up the returning user flow
      canvasSetupSequenceController_.StartSequence(CanvasSetupStartReason.SETTINGS_MENU);
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.DRAW_A_CIRCLE_WITH_PASSTHROUGH,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      SetActiveStepTransition(StepTransition.NEXT);
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.DRAW_A_CIRCLE_COMPLETE,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
    }

    [Test]
    public void TestReturningOlympusWithoutStylusCalibrationFlow() {
      inputMethodService_.IsUsingOlympusControllers().Returns(true);
      canvasSetupSequenceController_.ResetDeskCanvasSetupCompleted();
      canvasSetupSequenceController_.EndSequence();
      canvasSetupSequenceController_.StartSequence(CanvasSetupStartReason.SETTINGS_MENU);
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.ATTACH_STYLUS_TIP,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      SetActiveStepTransition(StepTransition.NEXT);
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.TAP_DESK_WITH_PASSTHROUGH,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      SetActiveStepTransition(StepTransition.NEXT);
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.DRAW_A_CIRCLE_WITH_PASSTHROUGH,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      SetActiveStepTransition(StepTransition.NEXT);
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.DRAW_A_CIRCLE_COMPLETE,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
    }

    [Test]
    public void TestReturningOlympusWithStylusTipsCalibrationFlow() {
      inputMethodService_.IsUsingOlympusControllers().Returns(true);
      canvasSetupSequenceController_.SetStylusTipPressedInLastCalibration();
      canvasSetupSequenceController_.EndSequence();
      canvasSetupSequenceController_.StartSequence(CanvasSetupStartReason.SETTINGS_MENU);
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.TAP_DESK_WITH_PASSTHROUGH,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      dispatcher_.Dispatch(new DeskCanvasHeightDetected());
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.DRAW_A_CIRCLE_COMPLETE,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
    }

    [Test]
    public void TestStylusTouchOnDrawACircleStep() {
      canvasSetupSequenceController_.EndSequence();
      canvasSetupSequenceController_.StartSequence(CanvasSetupStartReason.SETTINGS_MENU);
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.DRAW_A_CIRCLE_WITH_PASSTHROUGH,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      dispatcher_.Dispatch(new DeskCanvasHeightDetected());
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.DRAW_A_CIRCLE_COMPLETE,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
    }

    [Test]
    public void TestAttachStylusTipsStep() {
      // Checks that the ATTACH_STYLUS_TIP step only appears if the
      // stylus tips weren't detected during the last calibration
      inputMethodService_.IsUsingOlympusControllers().Returns(true);
      canvasSetupSequenceController_.ResetDeskCanvasSetupCompleted();
      canvasSetupSequenceController_.EndSequence();
      canvasSetupSequenceController_.StartSequence(CanvasSetupStartReason.SETTINGS_MENU);
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.ATTACH_STYLUS_TIP,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      canvasSetupSequenceController_.SetStylusTipPressedInLastCalibration();
      canvasSetupSequenceController_.EndSequence();
      canvasSetupSequenceController_.StartSequence(CanvasSetupStartReason.SETTINGS_MENU);
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.TAP_DESK_WITH_PASSTHROUGH,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
    }

    [Test]
    public void TestCancelDeskCalibration() {
      canvasSetupSequenceController_.EndSequence();
      canvasSetupSequenceController_.StartSequence(CanvasSetupStartReason.SETTINGS_MENU);
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.DRAW_A_CIRCLE_WITH_PASSTHROUGH,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      SetActiveStepTransition(StepTransition.CLOSE);
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.INACTIVE,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
    }

    [Test]
    public void TestCalibrationFlowDoesNotChangeIfOlympusDetectedDuringSetup() {
      canvasSetupSequenceController_.ResetDeskCanvasSetupCompleted();
      canvasSetupSequenceController_.StartSequence(CanvasSetupStartReason.SETTINGS_MENU);
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.SETUP_INTRO,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      SetActiveStepTransition(StepTransition.NEXT);
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.CLEAR_YOUR_DESK,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
      inputMethodService_.IsUsingOlympusControllers().Returns(true);
      SetActiveStepTransition(StepTransition.NEXT);
      canvasSetupSequenceController_.UpdateStep();
      Assert.AreEqual(
        CanvasSetupSequenceController.StepType.TAP_DESK,
        canvasSetupSequenceController_.ActiveStep.StepType
      );
    }
  }
}
