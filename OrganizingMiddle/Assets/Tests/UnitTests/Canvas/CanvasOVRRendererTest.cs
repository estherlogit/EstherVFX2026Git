// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Collections.Generic;
using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using Facebook.SocialVR.Modules.RendererMerge;
using Facebook.Workrooms.Canvas;
using Facebook.Workrooms.Canvas.Definitions;
using Facebook.Workrooms.RTI.Calls.Renderer;
using Facebook.Workrooms.Whiteboard;
using Facebook.Xplat.Events;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Facebook.Workrooms.Tests.UnitTests.Whiteboard {

  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class CanvasOVRRendererTest {
    private IServiceContainer container_;
    private CanvasOVRRenderer canvasOVRRenderer_;
    private IOVROverlayController ovrOverlayController_;
    private IMergedRendererResolutionController resolutionController_;
    private IMergedRendererOVROverlayUsageController ovrOverlayUsageController_;
    private bool textureRequested_;

    [SetUp]
    public void Setup() {
      container_ = ServiceLocator.CreateServiceContainer(SceneManager.GetActiveScene().path);
      var logger = Substitute.For<ILogService>();
      container_.Bind<ILogService>().To(logger);
      resolutionController_ = Substitute.For<IMergedRendererResolutionController>();
      ovrOverlayUsageController_ = Substitute.For<IMergedRendererOVROverlayUsageController>();
      ovrOverlayController_ = Substitute.For<IOVROverlayController>();
      canvasOVRRenderer_ = new CanvasOVRRenderer(
        ovrRenderer: ovrOverlayController_,
        resolutionController: resolutionController_,
        ovrOverlayUsageController: ovrOverlayUsageController_
      );
      textureRequested_ = false;
      canvasOVRRenderer_.TextureUpdateRequired += () => { textureRequested_ = true; };
    }

    [TearDown]
    public void TearDown() {
      ServiceLocator.Clear();
      container_.Clear();
      ServiceLocator.RemoveServiceContainer(SceneManager.GetActiveScene().path);
      textureRequested_ = false;
    }

    [Test]
    public void ResolutionChangeInvokesTextureRequest() {
      resolutionController_.ValuesChanged += Raise.Event<Action>();
      Assert.AreEqual(true, textureRequested_);
    }

    [Test]
    public void OVRUsageChangeInvokesTextureRequest() {
      ovrOverlayUsageController_.ValuesChanged += Raise.Event<Action>();
      Assert.AreEqual(true, textureRequested_);
    }

    [Test]
    public void OVROverlayControllerTextureRequestWrapped() {
      ovrOverlayController_.TextureUpdateRequired += Raise.Event<Action>();
      Assert.AreEqual(true, textureRequested_);
    }

    [Test]
    public void UpdatingWithIncorrectResolutionSizeInvokesTextureRequest() {
      resolutionController_.Width.Returns(200);
      resolutionController_.Height.Returns(200);
      MockReceiveRenderTexture(100, 100);
      Assert.AreEqual(true, textureRequested_);
    }

    [Test]
    public void UpdatingWithCorrectResolutionSizeDoesNotInvokeTextureRequest() {
      resolutionController_.Width.Returns(100);
      resolutionController_.Height.Returns(100);
      MockReceiveRenderTexture(100, 100);
      Assert.AreEqual(false, textureRequested_);
    }

    [Test]
    public void CanvasOVRRendererWaitsUntilCorrectResolutionToUpdateOVRRenderingType() {
      ovrOverlayUsageController_.UseNonOVRRendering.Returns(true);

      resolutionController_.Width.Returns(200);
      resolutionController_.Height.Returns(200);
      MockReceiveRenderTexture(100, 100);
      ovrOverlayUsageController_.ValuesChanged += Raise.Event<Action>();
      // Did not apply OVR overlay usage change because resolution not yet updated; we don't
      // want to allocate a giant texture for a new OVR rendering type if a new resolution render texture is incoming
      ovrOverlayController_.DidNotReceive().UpdateTexture(Arg.Any<RenderTexture>());
      ovrOverlayController_.DidNotReceive().OverrideUseNonOVRRendering = true;

      MockReceiveRenderTexture(200, 200);
      ovrOverlayUsageController_.ValuesChanged += Raise.Event<Action>();
      // Now we allocate the new texture, resolution is correct, and OVR rendering type updated
      ovrOverlayController_.Received().UpdateTexture(Arg.Any<RenderTexture>());
      ovrOverlayController_.Received().OverrideUseNonOVRRendering = true;
    }

    private void MockReceiveRenderTexture(int width, int height) {
      canvasOVRRenderer_.UpdateTexture(new RenderTexture(width, height, 0));
    }
  }
}
