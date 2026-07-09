// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using Facebook.SocialVR.Core.Bedrock;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.Canvas.Definitions;
using Facebook.Workrooms.Canvas.SetupSequence;
using Facebook.Workrooms.Gatekeeper;
using Facebook.Workrooms.Surfaces;
using Facebook.Workrooms.Utils;
using Facebook.Xplat.Events;
using Facebook.Xplat.Gatekeeper;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Facebook.Workrooms.Tests.UnitTests.Whiteboard {

  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class CanvasInputStateControllerTest {
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
      mockWorkroom_.Dispose();
    }

    [Test]
    public void CreatingAStickyNoteAtDeskUsesTemporaryTransaction() {
      // We want sticky notes at the wall to only be local.
      // Later user can hit confirm to show the sticky note
      mockWorkroom_.DeskCanvas.CanvasElementCreator.ElementCreated +=
        args => Assert.AreEqual(true, args.StartTransaction);
      mockWorkroom_.DeskCanvas.InputStateController.CreateStickyNote();

      mockWorkroom_.DeskCanvas.InputStateController.AnnotationConfirmClicked();
      mockWorkroom_.DeskCanvas.Canvas.Received().CommitTransaction(Arg.Any<ulong>());
    }

    [Test]
    public void CreatingAStickyNoteAtWallUsesTemporaryTransaction() {
      // We want sticky notes at the wall to only be local.
      // Later user can hit confirm to show the sticky note
      mockWorkroom_.WallCanvas.CanvasElementCreator.ElementCreated +=
        args => Assert.AreEqual(true, args.StartTransaction);
      mockWorkroom_.WallCanvas.InputStateController.CreateStickyNote();
    }

    [Test]
    public void SelectingElementDuringCreationDoesNotCommit() {
      ICanvasElement createdCanvasElement = null;
      mockWorkroom_.WallCanvas.CanvasElementCreator.ElementCreated += (args) => {
        createdCanvasElement = args.CanvasElement;
        // Mock the current flow, which auto-focuses annotation mode on newly created elements
        mockWorkroom_.WallCanvas.InputStateController.ChangeAnnotatingElement(createdCanvasElement);
      };
      mockWorkroom_.WallCanvas.InputStateController.CreateStickyNote();
      // Ensure newly created element is not already committed
      mockWorkroom_.WallCanvas.Canvas.DidNotReceive().CommitTransaction(Arg.Is(createdCanvasElement.CanvasEntityId));
    }

    [Test]
    public void ImplicitConfirmCommitsTransactions() {
      var element1 = mockWorkroom_.WallCanvas.CanvasElementCreator.CreateElement(1);
      var element2 = mockWorkroom_.WallCanvas.CanvasElementCreator.CreateElement(2);
      mockWorkroom_.WallCanvas.InputStateController.ChangeAnnotatingElement(element1);
      mockWorkroom_.WallCanvas.Canvas.Received().StartTransaction(Arg.Is(element1.CanvasEntityId));
      mockWorkroom_.WallCanvas.InputStateController.ChangeAnnotatingElement(element2);
      mockWorkroom_.WallCanvas.Canvas.Received().CommitTransaction(Arg.Is(element1.CanvasEntityId));
      mockWorkroom_.WallCanvas.Canvas.Received().StartTransaction(Arg.Is(element2.CanvasEntityId));
      mockWorkroom_.WallCanvas.InputStateController.AnnotationConfirmClicked();
      mockWorkroom_.WallCanvas.Canvas.Received().CommitTransaction(Arg.Is(element2.CanvasEntityId));
    }

    [Test]
    public void ChangingToKeyboardComposeModeDoesNotCommit() {
      var element1 = mockWorkroom_.DeskCanvas.CanvasElementCreator.CreateElement(1);
      mockWorkroom_.DeskCanvas.InputStateController.ChangeAnnotatingElement(element1);
      mockWorkroom_.DeskCanvas.Canvas.Received().StartTransaction(Arg.Is(element1.CanvasEntityId));
      mockWorkroom_.DeskCanvas.InputStateController.ToggleKeyboardInput();
      mockWorkroom_.DeskCanvas.Canvas.DidNotReceive().CommitTransaction(Arg.Is(element1.CanvasEntityId));
      mockWorkroom_.DeskCanvas.InputStateController.AnnotationConfirmClicked();
      mockWorkroom_.DeskCanvas.Canvas.Received().CommitTransaction(Arg.Is(element1.CanvasEntityId));
    }

    [Test]
    public void IfDeskNotCalibratedAndTextInputDisabledAnnotationModeDoesNotActivate() {
      var element1 = mockWorkroom_.DeskCanvas.CanvasElementCreator.CreateElement(1);
      mockWorkroom_.DeskCanvas.DevicePlatform.IsVREnabled().ReturnsForAnyArgs(true);
      mockWorkroom_.DeskCanvas.InputStateController.ChangeAnnotatingElement(element1);
      mockWorkroom_.DeskCanvas.Canvas.DidNotReceive().StartTransaction(Arg.Is(element1.CanvasEntityId));
    }

    [Test]
    public void IfDeskNotCalibratedAndTextInputDisabledThenDoesNotCreateStickyNote() {
      mockWorkroom_.DeskCanvas.DevicePlatform.IsVREnabled().ReturnsForAnyArgs(true);
      mockWorkroom_.DeskCanvas.CanvasElementCreator.ElementCreated +=
      args => Assert.AreEqual(false, args.StartTransaction);
      mockWorkroom_.DeskCanvas.InputStateController.CreateStickyNote();
    }

    [Test]
    public void IfDeskNotCalibratedAndTextInputEnabledThenCreatesStickyNote() {
      mockWorkroom_.DeskCanvas.DevicePlatform.IsVREnabled().ReturnsForAnyArgs(true);
      mockWorkroom_.DeskCanvas.CanvasElementCreator.ElementCreated +=
        args => Assert.AreEqual(true, args.StartTransaction);
      mockWorkroom_.DeskCanvas.InputStateController.CreateStickyNote();
    }

    private class MockWorkroom : IDisposable {
      public MockCanvas DeskCanvas { get; }
      public MockCanvas WallCanvas { get; }

      public MockWorkroom() {
        DeskCanvas = new MockCanvas(WorkroomsLocation.DESK);
        WallCanvas = new MockCanvas(WorkroomsLocation.WALL);
      }

      public class MockCanvas : IDisposable {
        private readonly GameObject rootWhiteboardGameObject_;
        private readonly GameObject canvasAnchor_;

        public ICanvasInputStateController InputStateController { get; }
        public MockCanvasElementCreator CanvasElementCreator { get; }
        public ICanvas Canvas { get; }
        public ICanvasInputController CanvasInputController { get; }
        public IDevicePlatform DevicePlatform { get; }
        public IGatekeeperClient GKClient { get; }

        public MockCanvas(WorkroomsLocation location) {
          rootWhiteboardGameObject_ = new GameObject();

          canvasAnchor_ = new GameObject();

          Canvas = Substitute.For<ICanvas>();
          Canvas.CanvasAnchor.Returns(canvasAnchor_.transform);

          var canvasConfig = Substitute.For<ICanvasConfig>();
          var stickyNotesConfig = Substitute.For<StickyNotesConfig>();
          canvasConfig.StickyNotesElementConfig.Returns(stickyNotesConfig);

          CanvasInputController = Substitute.For<ICanvasInputController>();
          DevicePlatform = Substitute.For<IDevicePlatform>();
          GKClient = Substitute.For<IGatekeeperClient>();

          CanvasElementCreator = new MockCanvasElementCreator(Canvas);

          InputStateController = new CanvasInputStateController(
            Substitute.For<IDispatcher>(),
            CanvasInputController,
            location,
            canvasConfig,
            rootWhiteboardGameObject_.transform,
            CanvasElementCreator,
            Substitute.For<IRendererWrapper>(),
            Substitute.For<ICanvasSetupController>(),
            DevicePlatform,
            Substitute.For<ICanvasInstanceService>(),
            Substitute.For<ICanvasDriver>(),
            new MockDOTweenService(),
            GKClient
          );
        }

        public void Dispose() {
          Object.DestroyImmediate(rootWhiteboardGameObject_);
          Object.DestroyImmediate(canvasAnchor_);
          InputStateController?.Dispose();
        }
      }

      public void Dispose() {
        DeskCanvas.Dispose();
        WallCanvas.Dispose();
      }

      public class MockCanvasElementCreator : ICanvasElementCreator {
        public event Action<ElementCreatedEventArgs> ElementCreated;
        private ulong nextElementID_ = 100;
        public ICanvas Canvas { get; }

        public MockCanvasElementCreator(ICanvas canvas) {
          Canvas = canvas;
        }

        public ICanvasElement TryCreateStickyNote(bool startTransaction) {
          return CreateElement(nextElementID_++, startTransaction);
        }

        public ICanvasElement CreateElement(ulong ID, bool startTransaction = false) {
          var canvasElement = Substitute.For<ICanvasElement>();
          canvasElement.Canvas.Returns(Canvas);
          canvasElement.CanvasEntityId.Returns(ID);
          ElementCreated?.Invoke(
            new ElementCreatedEventArgs() {
              CanvasElement = canvasElement,
              StartTransaction = startTransaction
            }
          );
          return canvasElement;
        }

        public struct ElementCreatedEventArgs {
          public ICanvasElement CanvasElement { get; set; }
          public bool StartTransaction { get; set; }
        }
      }
    }
  }
}
