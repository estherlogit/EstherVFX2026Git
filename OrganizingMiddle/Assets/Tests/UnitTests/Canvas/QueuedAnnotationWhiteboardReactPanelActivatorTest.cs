// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections.Generic;
using Facebook.SocialVR.Core.Bedrock;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using Facebook.SocialVR.Core.Utils;
using Facebook.SocialVR.Modules.RendererMerge;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.Canvas.Definitions;
using Facebook.Workrooms.Surfaces;
using Facebook.Workrooms.Whiteboard;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Facebook.Workrooms.Tests.UnitTests.Whiteboard {

  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class QueuedAnnotationWhiteboardReactPanelActivatorTest {

    private const int MAX_PANEL_ACTIVATING_AT_A_TIME = 1;
    private const int NUM_PANELS = MAX_PANEL_ACTIVATING_AT_A_TIME + 2;
    private IServiceContainer container_;
    private QueuedAnnotationWhiteboardReactPanelActivator panelActivator_;
    private FakeAsyncInitPanel[] panels_;

    [SetUp]
    public void Setup() {
      container_ = ServiceLocator.CreateServiceContainer(SceneManager.GetActiveScene().path);
      var logger = Substitute.For<ILogService>();
      container_.Bind<ILogService>().To(logger);
      var updateRunner = Substitute.For<IUpdateRunner>();

      var config = Substitute.For<IPanelActivatorConfig>();
      config.WhiteboardReactStaggerSetActive.Returns(MAX_PANEL_ACTIVATING_AT_A_TIME);

      panelActivator_ = new QueuedAnnotationWhiteboardReactPanelActivator(updateRunner, config);

      panels_ = new FakeAsyncInitPanel[NUM_PANELS];
      for (int i = 0; i < panels_.Length; i++) {
        panels_[i] = CreatePanel();
      }

      ;
    }

    [TearDown]
    public void TearDown() {
      ServiceLocator.Clear();
      container_.Clear();
      ServiceLocator.RemoveServiceContainer(SceneManager.GetActiveScene().path);
      ForEachPanel(
        0,
        NUM_PANELS,
        panel => {
          if (!panel.IsDestroyedOrNull()) {
            Object.DestroyImmediate(panel.gameObject);
          }
        }
      );
    }

    [Test]
    public void QueuedPanelIsSetActiveWhenEarlierPanelInitialized() {
      // All panels request to set active, but none will be active yet until Update
      ForEachPanel(0, NUM_PANELS, panel => panelActivator_.RequestSetActive(panel));
      AssertPanelsAreActive(0, NUM_PANELS, false);

      // First set panels set active after update
      panelActivator_.RunUpdate(0);
      AssertPanelsAreActive(0, MAX_PANEL_ACTIVATING_AT_A_TIME, true);
      // Remaining panels still inactive
      AssertPanelsAreActive(MAX_PANEL_ACTIVATING_AT_A_TIME, MAX_PANEL_ACTIVATING_AT_A_TIME, false);

      // One panel completes initialisation
      panels_[0].SetInitialized();

      // Now one extra panel is made active
      panelActivator_.RunUpdate(0);
      AssertPanelsAreActive(MAX_PANEL_ACTIVATING_AT_A_TIME, MAX_PANEL_ACTIVATING_AT_A_TIME, true);
    }

    [Test]
    public void AlreadyInitializedPanelsDoNotConsumeQueue() {
      // Request panels be set active, but first is already initialized
      panels_[0].IsInitialized = true;
      ForEachPanel(0, NUM_PANELS, panel => panelActivator_.RequestSetActive(panel));

      // First set of panels set active after update
      panelActivator_.RunUpdate(0);
      AssertPanelsAreActive(0, MAX_PANEL_ACTIVATING_AT_A_TIME, true);

      // Additional panel is active because panel 0 was already initialized
      AssertPanelsAreActive(MAX_PANEL_ACTIVATING_AT_A_TIME, 1, true);
    }

    [Test]
    public void PanelsCanCancelRequests() {
      ForEachPanel(0, NUM_PANELS, panel => panelActivator_.RequestSetActive(panel));
      panelActivator_.RunUpdate(0);

      // Next panel is waiting
      AssertPanelsAreActive(MAX_PANEL_ACTIVATING_AT_A_TIME, 1, false);

      // Next is set active when early panel activation is cancelled
      panelActivator_.CancelRequest(panels_[0]);
      panelActivator_.RunUpdate(0);
      AssertPanelsAreActive(MAX_PANEL_ACTIVATING_AT_A_TIME, 1, true);
    }

    [Test]
    public void PanelsCanCancelRequestBeforeTheyAreProcessed() {
      ForEachPanel(0, NUM_PANELS, panel => panelActivator_.RequestSetActive(panel));
      panelActivator_.CancelRequest(panels_[0]);
      panelActivator_.RunUpdate(0);

      // Cancelled panel is skipped, so subsequent panel is made active
      AssertPanelsAreActive(MAX_PANEL_ACTIVATING_AT_A_TIME, 1, true);
    }

    [Test]
    public void ReRequestingSamePanelSetsItActiveImmediately() {
      // Panel set active request made
      panelActivator_.RequestSetActive(panels_[0]);
      panelActivator_.RunUpdate(0);
      Assert.AreEqual(true, panels_[0].gameObject.activeSelf);

      // Elsewhere, panel is set inactive and another request is made
      panels_[0].gameObject.SetActive(false);
      panelActivator_.RequestSetActive(panels_[0]);

      // Panel set active immediately
      Assert.AreEqual(true, panels_[0].gameObject.activeSelf);
    }

    [Test]
    public void DestroyedQueuedPanelsDoNotCauseErrors() {
      // Queue all but one panel
      ForEachPanel(0, NUM_PANELS - 1, panel => panelActivator_.RequestSetActive(panel));

      // Run update to start initializing panel 0
      panelActivator_.RunUpdate(0);

      // Destroy all queued panels
      ForEachPanel(
        MAX_PANEL_ACTIVATING_AT_A_TIME,
        NUM_PANELS - MAX_PANEL_ACTIVATING_AT_A_TIME - 1,
        panel => {
          var obj = panel.gameObject;
          Object.DestroyImmediate((FakeAsyncInitPanel)panel);
          Object.DestroyImmediate(obj);
        }
      );

      // First panel completes initialization
      panels_[0].SetInitialized();

      // Run update should not cause errors
      panelActivator_.RunUpdate(0);

      // Queue the final panel
      ForEachPanel(NUM_PANELS - 1, 1, panel => panelActivator_.RequestSetActive(panel));

      // Run update should now set that last panel active
      panelActivator_.RunUpdate(0);

      // Assert non-destroyed panels (first and last) are active
      Assert.AreEqual(true, panels_[0].gameObject.activeSelf);
      Assert.AreEqual(true, panels_[NUM_PANELS - 1].gameObject.activeSelf);
    }

    private void AssertPanelsAreActive(int start, int count, bool isActive) {
      ForEachPanel(start, count, p => { Assert.AreEqual(isActive, p.gameObject.activeSelf); });
    }

    private void ForEachPanel(int start, int count, Action<IAsyncInitPanel> action) {
      for (var index = start; index < start + count; index++) {
        action(panels_[index]);
      }
    }

    private FakeAsyncInitPanel CreatePanel() {
      var gObj = new GameObject();
      var panel = gObj.AddComponent<FakeAsyncInitPanel>();
      panel.IsInitialized = false;
      panel.gameObject.SetActive(false);
      return panel;
    }

    private class FakeAsyncInitPanel : MonoBehaviour, IAsyncInitPanel {
#pragma warning disable CS0067 // Disable unused event error
      public event Action<IAsyncInitPanel> Initialized;
#pragma warning restore CS0067
      public bool IsInitialized { get; set; }

      public void SetInitialized() {
        Initialized?.Invoke(this);
      }
    }
  }
}
