// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections;
using Facebook.SocialVR.Core.Services;
using Facebook.SocialVR.Core.OnCall;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Facebook.Workrooms.Testing;
using Facebook.Xplat.Events;
using Facebook.SocialVR.Core.Bedrock;
using Facebook.Workrooms.Application;
using Facebook.Workrooms.Services;
using Facebook.Workrooms.Tests.Tests.Initialization;
using TBE;
using System;
using System.Runtime.InteropServices;
using AOT;
using System.Collections.Generic;
using Facebook.Xplat.Analytics;
using Facebook.Xplat.Gatekeeper;
using Facebook.Xplat.Threading;

namespace Facebook.Workrooms.Tests.Tests.Audio {
  [OnCall(OnCallName.workrooms_core)]
  public class AudioPlaybackTest {
    private int samplingRate_ = 48000;
    private ToneGenerator toneGenerator_;

    private ServiceInitializationTestUtils testUtils_;
    private AudioEngineManager.Client audioEngineManagerClient_;

    [SetUp]
    public void Setup() {
      testUtils_ = new ServiceInitializationTestUtils(new List<ServiceGraphProperty>() { ServiceGraphProperty.HasLocalPlayer });

      var dispatcher = new DispatcherSpyDecorator(new Dispatcher(), Substitute.For<IDispatcher>());
      testUtils_.ServiceContainer.Bind<IDispatcher>().To(dispatcher);


      var runtimeConf = Substitute.For<IRuntimeContextConfig>();
      runtimeConf.targetEnvironment.Returns(TargetEnvironment.TEST);
      testUtils_.ServiceContainer.Bind<RuntimeContext>().To(new RuntimeContext(runtimeConf));

      var testRoot = new GameObject();
      var devicePlatformMock = Substitute.For<IDevicePlatform>();
      devicePlatformMock.Device.Returns(HardwareDevice.META_QUEST_PRO);
      testUtils_.ServiceContainer.Bind<IDevicePlatform>().To(devicePlatformMock);
      testUtils_.ServiceContainer.Bind<IServicesMonoBehaviourManager>().To(new ServicesMonoBehaviourManager(testRoot));
      testUtils_.ServiceContainer.Bind<OCGatekeeperClient>().To(new OCGatekeeperClient(null));
      testUtils_.ServiceContainer.Bind<IMainThreadExecutor>().To(Substitute.For<IMainThreadExecutor>());
      testUtils_.ServiceContainer.Bind<IAnalytics>().To(Substitute.For<IAnalytics>());

      var defaultConfig = new WorkroomsConfigurationOptions(
        new[] { "--skip-nux", "--create-default-desk", "--create-default-whiteboard", "--enable-desktop-mode" }
      );
      testUtils_.ServiceContainer.Bind<IWorkroomsConfigurationOptions>().To(defaultConfig);

      toneGenerator_ = new ToneGenerator(440, samplingRate_);

      testUtils_.Run(typeof(WorkroomsAudioBootstrapperServiceProvider));

      var bootstrapperService = testUtils_.ServiceContainer.TryGet<WorkroomsAudioBootstrapperService>();

      audioEngineManagerClient_ = bootstrapperService.FBAudioBootstrapper.AudioEngineManager.NewClient();
    }

    [TearDown]
    public void Cleanup() {
      testUtils_.Dispose();
      testUtils_ = null;
    }

    [UnityTest]
    public IEnumerator MonoChannelAudioQueueTest() {
      if (audioEngineManagerClient_.GetNativeEngine(out var engine)) {
        var data = toneGenerator_.GetNextSamples((int) samplingRate_ * 3);
        engine.setEnableLoudness(true);
        
        var audioMixBuffer = new float[engine.getBufferSize() * 2];
        var audioObject = engine.createAudioObject();
        audioObject.openQueue(ChannelMap.MONO, PCMType.FLOAT, (uint) (3 * samplingRate_));

        audioObject.enqueueData(data, data.Length, ChannelMap.MONO);
        audioObject.play();

        // Process the audio graph for 30 frames
        for (int i = 0; i < 30; i++)
        {
          engine.getAudioMix(audioMixBuffer, engine.getBufferSize(), 2);
        }
        audioObject.close();
        AssertLoudnessIsNotZero(engine.getRenderedLoudness());
      } else {
        throw new Exception("Could not get native audio engine");
      };
      yield return null;
    }

    [UnityTest]
    public IEnumerator MonoChannelAudioCallbackTest() {

      if (audioEngineManagerClient_.GetNativeEngine(out var engine)) {
        engine.setEnableLoudness(true);
        var handle = GCHandle.Alloc(toneGenerator_);
        var audioMixBuffer = new float[engine.getBufferSize() * 2];
        var audioObject = engine.createAudioObject();
        audioObject.setAudioBufferCallback(AudioCallback, 1, ChannelMap.MONO, GCHandle.ToIntPtr(handle));

        audioObject.play();

        // Process the audio graph for 30 frames
        for (int i = 0; i < 30; i++) {
          engine.getAudioMix(audioMixBuffer, engine.getBufferSize(), 2);
        }

        audioObject.close();
        AssertLoudnessIsNotZero(engine.getRenderedLoudness());
        handle.Free();
      } else {
        throw new Exception("Could not get native audio engine");
      };
      yield return null;
    }

    [MonoPInvokeCallback(typeof(AudioObjectBufferCallback))]
    private static void AudioCallback(IntPtr floatInterleavedAudio, uint numSamplesInAllChannels, uint numChannels, IntPtr userData) {
      var toneGenerator = GCHandle.FromIntPtr(userData).Target as ToneGenerator;

      var length = (int)(numSamplesInAllChannels * numChannels);
      var samples = toneGenerator.GetNextSamples(length);
      Marshal.Copy(samples, 0, floatInterleavedAudio, length);
    }

    private static void AssertLoudnessIsNotZero(LoudnessStatistics loudness) {
      Assert.IsFalse(float.IsNegativeInfinity(loudness.integrated));
      Assert.IsFalse(float.IsNegativeInfinity(loudness.shortTerm));
      Assert.IsFalse(float.IsNegativeInfinity(loudness.momentary));
      Assert.IsFalse(loudness.truePeak == 0);
    }
  }
}
