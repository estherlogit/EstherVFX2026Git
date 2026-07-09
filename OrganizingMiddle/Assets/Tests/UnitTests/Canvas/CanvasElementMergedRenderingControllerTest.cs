// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections.Generic;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using Facebook.SocialVR.Modules.RendererMerge;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.Canvas.Definitions;
using Facebook.Workrooms.Surfaces;
using Facebook.Workrooms.Whiteboard;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Facebook.Workrooms.Tests.UnitTests.Whiteboard {

  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class CanvasElementMergedRenderingControllerTest {
    private const int DEFAULT_LAYER = 0;
    private const int MERGE_LAYER = 2;
    private const int FIRST_ELEMENT_ID = 351;
    private const int NUM_ELEMENTS = 2;

    private IServiceContainer container_;
    private MockWorkroom mockWorkroom_;

    [SetUp]
    public void Setup() {
      container_ = ServiceLocator.CreateServiceContainer(SceneManager.GetActiveScene().path);
      var logger = Substitute.For<ILogService>();
      container_.Bind<ILogService>().To(logger);
      mockWorkroom_ = new MockWorkroom();
    }

    [TearDown]
    public void TearDown() {
      ServiceLocator.Clear();
      container_.Clear();
      ServiceLocator.RemoveServiceContainer(SceneManager.GetActiveScene().path);
    }

    [Test]
    public void IndividualRenderersDisabledWhenMerged() {
      mockWorkroom_.CanvasElementMerger.MockMergerStatusUpdate(mergeStatus: MergeStatus.MERGED);
      AssertRendererStatus(WorkroomsLocation.DESK, individualRendererVisible: false, mergeStatus: MergeStatus.MERGED);
      AssertRendererStatus(WorkroomsLocation.WALL, individualRendererVisible: false, mergeStatus: MergeStatus.MERGED);
    }

    [Test]
    public void IndividualRenderersEnabledWhenNotMerged() {
      mockWorkroom_.CanvasElementMerger.MockMergerStatusUpdate(mergeStatus: MergeStatus.UNMERGED);
      AssertRendererStatus(WorkroomsLocation.DESK, individualRendererVisible: true, mergeStatus: MergeStatus.UNMERGED);
      AssertRendererStatus(WorkroomsLocation.WALL, individualRendererVisible: true, mergeStatus: MergeStatus.UNMERGED);
    }

    [Test]
    public void IndividualRenderersEnabledForAnnotationMode() {
      mockWorkroom_.CanvasElementMerger.MockMergerStatusUpdate(mergeStatus: MergeStatus.MERGED);
      MockAnnotationModeStartingAtDesk();
      AssertRendererStatus(WorkroomsLocation.DESK, individualRendererVisible: true, mergeStatus: MergeStatus.UNMERGED);
      // Wall renderer also unmerges, so that element is not visible on desk in its merged
      // position beneath annotation popup out
      AssertRendererStatus(WorkroomsLocation.WALL, individualRendererVisible: true, mergeStatus: MergeStatus.UNMERGED);
    }

    [Test]
    public void DeskRenderersEnabledWhenMerging() {
      mockWorkroom_.CanvasElementMerger.MockMergerStatusUpdate(mergeStatus: MergeStatus.MERGING);
      // Not yet merged, so desk renderer still needs to be visible
      AssertRendererStatus(WorkroomsLocation.DESK, individualRendererVisible: true, mergeStatus: MergeStatus.MERGING);
    }

    [Test]
    public void WallIndividualRenderersInvisibleButFlagAsUsedWhenMerging() {
      mockWorkroom_.CanvasElementMerger.MockMergerStatusUpdate(mergeStatus: MergeStatus.MERGING);
      // wall element only visible for the remerging snapshot, not by main camera
      // but is 'being used' for merge, ie, needs proper z/depth layer
      AssertRendererStatus(WorkroomsLocation.WALL, individualRendererVisible: false, mergeStatus: MergeStatus.MERGING);
    }

    [Test]
    public void DontUnmergeElementForFirstAnnotationsDrawing() {
      DontUnmergeElementForFirstAnnotationsDrawing(WorkroomsLocation.DESK);
      DontUnmergeElementForFirstAnnotationsDrawing(WorkroomsLocation.WALL);
    }

    private void DontUnmergeElementForFirstAnnotationsDrawing(WorkroomsLocation location) {
      var canvas = location == WorkroomsLocation.DESK ? mockWorkroom_.DeskCanvas : mockWorkroom_.WallCanvas;
      var canvasElement0 = canvas.MockCanvasElements[0];
      var canvasElement1 = canvas.MockCanvasElements[1];
      // Initial state, renderers not active
      SetDrawingActiveAndAssertIndividualRendererState(canvasElement0, drawingActive: false, rendererActive: false);
      // First 'drawing active' event, ignore this since this is called when each react panel is initialising,
      // and does not really require renderer to display individually (ie, is not really actively receiving drawings)
      SetDrawingActiveAndAssertIndividualRendererState(canvasElement0, drawingActive: true, rendererActive: false);
      // Initial drawing state finished
      SetDrawingActiveAndAssertIndividualRendererState(canvasElement0, drawingActive: false, rendererActive: false);
      // For subsequent drawing states, we do show renderer individually, since this really does mean that drawing is active
      SetDrawingActiveAndAssertIndividualRendererState(canvasElement0, drawingActive: true, rendererActive: true);
      // Check that other element on canvas stays merged
      SetDrawingActiveAndAssertIndividualRendererState(canvasElement1, drawingActive: false, rendererActive: false);
      // Drawing state complete
      SetDrawingActiveAndAssertIndividualRendererState(canvasElement0, drawingActive: false, rendererActive: false);
    }

    private void SetDrawingActiveAndAssertIndividualRendererState(
      MockElement canvasElement,
      bool drawingActive,
      bool rendererActive
    ) {
      canvasElement.AnnotationView.IsPanelActive.Returns(drawingActive);
      canvasElement.AnnotationView.IsPanelActiveChanged += Raise.Event<Action>();
      canvasElement.AssertIndividualRendererVisible(rendererActive);
    }

    private void AssertRendererStatus(
      WorkroomsLocation location,
      bool individualRendererVisible,
      MergeStatus mergeStatus
    ) {
      var canvas = location == WorkroomsLocation.DESK ? mockWorkroom_.DeskCanvas : mockWorkroom_.WallCanvas;
      canvas.MockCanvasElements[0].AssertIndividualRendererVisible(individualRendererVisible);
      canvas.MockCanvasElements[0].AssertRendererMergeStatus(mergeStatus);
    }

    private void MockAnnotationModeStartingAtDesk() {
      var inputState = new CanvasInputStateTransitionStatus(
        CanvasInputStateMachine.State.DEFAULT,
        CanvasInputStateMachine.State.ANNOTATE
      );
      var deskCanvasInputStateController = mockWorkroom_.DeskCanvas.InputStateController;
      deskCanvasInputStateController.CurrentStatus.Returns(inputState);
      deskCanvasInputStateController.AnnotationModeActiveForCanvasElement.Returns(
        mockWorkroom_.DeskCanvas.MockCanvasElements[0].CanvasElement
      );
      deskCanvasInputStateController.StateUpdated += Raise.Event<Action<CanvasInputStateTransitionStatus>>(inputState);
    }

    private class MockWorkroom {
      public MockCanvas DeskCanvas { get; }
      public MockCanvas WallCanvas { get; }
      public MockCanvasElementMerger CanvasElementMerger { get; }

      public MockWorkroom() {
        CanvasElementMerger = new MockCanvasElementMerger();
        WallCanvas = new MockCanvas(WorkroomsLocation.WALL);
        DeskCanvas = new MockCanvas(WorkroomsLocation.DESK);

        var canvasInputStateControllers = new List<ICanvasInputStateController> {
          WallCanvas.InputStateController,
          DeskCanvas.InputStateController
        };
        for (var i = 0; i < NUM_ELEMENTS; i++) {
          var wallElement = WallCanvas.MockCanvasElements[i];
          var deskElement = DeskCanvas.MockCanvasElements[i];
          var elementInputControllers = new List<ICanvasElementInputController> {
            wallElement.CanvasElementInputController,
            deskElement.CanvasElementInputController
          };
          // init desk before wall to test edge case where wall element not yet existing for desk to
          // subscribe for merge events
          deskElement.InitMergeRenderingController(
            CanvasElementMerger,
            elementInputControllers,
            canvasInputStateControllers
          );
          wallElement.InitMergeRenderingController(
            CanvasElementMerger,
            elementInputControllers,
            canvasInputStateControllers
          );
        }
      }
    }

    private class MockCanvas {
      public ICanvasInputStateController InputStateController { get; }
      public List<MockElement> MockCanvasElements { get; }

      public MockCanvas(WorkroomsLocation location) {
        InputStateController = Substitute.For<ICanvasInputStateController>();
        MockCanvasElements = new List<MockElement>();
        for (ulong i = 0; i < NUM_ELEMENTS; i++) {
          MockCanvasElements.Add(new MockElement(location, FIRST_ELEMENT_ID + i));
        }
      }
    }

    private class MockElement {
      private readonly WorkroomsLocation location_;
      private readonly IsPanelActiveProvider annotationPanelIsActiveProvider_;
      private readonly IsPanelActiveProvider textPanelIsActiveProvider_;
      private MergeStatus? lastMergeStatusEventReceived_;

      public ICanvasElementInputController CanvasElementInputController { get; }
      public ITextureView TextureView { get; }
      public ICanvasElement CanvasElement { get; }
      public List<Renderer> Renderers { get; }
      public CanvasElementMergedRenderingController MergeRenderingController { get; private set; }
      public IReactPanelView AnnotationView { get; }

      public MockElement(WorkroomsLocation location, ulong elementID) {
        location_ = location;
        Renderers = new List<Renderer>() {
          new GameObject(location + "-" + elementID + "-0").AddComponent<MeshRenderer>(),
          new GameObject(location + "-" + elementID + "-1").AddComponent<MeshRenderer>()
        };
        CanvasElementInputController = Substitute.For<ICanvasElementInputController>();

        TextureView = Substitute.For<ITextureView>();
        // default to texture set, will test case when not set explicitly
        TextureView.IsTextureSet.Returns(true);

        CanvasElement = Substitute.For<ICanvasElement>();
        CanvasElement.CanvasElementId.Returns(elementID);
        CanvasElement.IsInteractedWithRemotely.Returns(false);

        AnnotationView = Substitute.For<IReactPanelView>();
        var textView = Substitute.For<IReactPanelView>();
        annotationPanelIsActiveProvider_ = new IsPanelActiveProvider(AnnotationView);
        textPanelIsActiveProvider_ = new IsPanelActiveProvider(textView);
      }

      public void InitMergeRenderingController(
        ICanvasElementMerger canvasElementMerger,
        List<ICanvasElementInputController> canvasElementInputControllers,
        List<ICanvasInputStateController> canvasInputStateControllers
      ) {
        MergeRenderingController = new CanvasElementMergedRenderingController(
          renderers: Renderers,
          elementMerger: canvasElementMerger,
          canvasElement: CanvasElement,
          location: location_,
          annotationPanelIsActiveProvider: annotationPanelIsActiveProvider_,
          textPanelIsActiveProvider: textPanelIsActiveProvider_,
          textureView: TextureView,
          canvasElementInputControllers,
          canvasInputStateControllers,
          Substitute.For<ICanvasDriver>()
        );

        MergeRenderingController.MergeStatusChanged += MergeStatusChanged;
        MergeRenderingController.UpdateRenderer();
      }

      public void AssertIndividualRendererVisible(bool visible) {
        // Assert the renderer is in the correct state
        foreach (var renderer in Renderers) {
          if (location_ == WorkroomsLocation.WALL) {
            Assert.AreEqual(visible ? DEFAULT_LAYER : MERGE_LAYER, renderer.gameObject.layer);
          } else {
            Assert.AreEqual(visible, renderer.enabled);
          }
        }
      }

      public void AssertRendererMergeStatus(MergeStatus mergeStatus) {
        // Assert this is correct; this is used to assign a z/depth layer for renderer only when used
        Assert.AreEqual(mergeStatus, lastMergeStatusEventReceived_.Value);
      }

      private void MergeStatusChanged(MergeStatus e) {
        lastMergeStatusEventReceived_ = e;
      }
    }

    /// <summary>
    /// Simple mocked version of <see cref="CanvasElementMerger"/> and internal classes such as the merging
    /// state machine <see cref="MergeableRendererContext"/>.
    /// </summary>
    private class MockCanvasElementMerger : ICanvasElementMerger {
      private readonly Dictionary<Renderer, RendererMergeOptions> mergedRenderers_ =
        new Dictionary<Renderer, RendererMergeOptions>();

      public CanvasElementMerger.CanvasElementRenderers TrackedCanvasElements { get; } =
        new CanvasElementMerger.CanvasElementRenderers();

      public void SetMergingEnabled(Renderer renderer, bool enabled, RendererMergeOptions? options = null) {
        if (enabled) {
          mergedRenderers_[renderer] = options ?? default;
          MockMergeStateForRenderer(renderer, MergeStatus.MERGED);
        } else {
          MockMergeStateForRenderer(renderer, MergeStatus.UNMERGED);
          mergedRenderers_.Remove(renderer);
        }
      }

      public void MockMergerStatusUpdate(MergeStatus mergeStatus) {
        foreach (var kvp in mergedRenderers_) {
          MockMergeStateForRenderer(kvp.Key, mergeStatus);
        }
      }

      private void MockMergeStateForRenderer(Renderer renderer, MergeStatus mergeStatus) {
        renderer.gameObject.layer = mergeStatus == MergeStatus.UNMERGED ? DEFAULT_LAYER : MERGE_LAYER;
        mergedRenderers_[renderer].MergeStatusChanged?.Invoke(mergeStatus);
      }

      public void PerformMerge() { }
      public bool IsLoading { get; }
#pragma warning disable CS0067
      public event Action LoadingStateChanged;
#pragma warning restore CS0067
    }

  }
}
