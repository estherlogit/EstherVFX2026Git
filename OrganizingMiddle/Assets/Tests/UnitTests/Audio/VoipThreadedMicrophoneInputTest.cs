// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using Facebook.SocialVR.Core.Logging;
using Facebook.SocialVR.Core.Services;
using Facebook.Workrooms.Testing;
using Facebook.Xplat.Events;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Facebook.SocialVR.Core.OnCall;
using Facebook.SocialVR.Core.Voip;
using Oculus.Platform;
using Facebook.Xplat.Threading;
using System;

namespace Facebook.Workrooms.Tests.UnitTests {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class VoipThreadedMicrophoneInputTest : MonoBehaviour {

    private FBAudioMicrophoneInput microphone_;
    private IDispatcher dispatcher_;
    private VoipThreadedMicrophoneInput voipThreadedMicrophoneInput_;

    private const int DefaultChunkSizeMs = VoipSamplesChunker.DEFAULT_SINGLE_CHUNK_LENGTH_MS; // 20 ms
    private const int DefaultChunkSize = VoipSamplesChunker.DEFAULT_MULTIPLE_CHUNK_SIZE; // 20 ms * 48 samples/ms = 960
    private readonly float[] PcmInput = new float[5 * DefaultChunkSize]; //Buffer that holds the pcm data

    [OneTimeSetUp]
    public void OneTimeSetup() {
      dispatcher_ = new DispatcherSpyDecorator(new Dispatcher(), Substitute.For<IDispatcher>());
      ServiceLocator.Bind<IDispatcher>().To(dispatcher_);

      var inputMethodService = Substitute.For<IVoipServiceManager>();
      ServiceLocator.Bind<IVoipServiceManager>().To(inputMethodService);

      var logger = Substitute.For<ILogService>();
      ServiceLocator.Bind<ILogService>().To(logger);

      var mainThreadExecutor = Substitute.For<IMainThreadExecutor>();
      ServiceLocator.Bind<IMainThreadExecutor>().To(mainThreadExecutor);

      microphone_ = Substitute.For<FBAudioMicrophoneInput>(logger.GetLog("dummy"));
      ServiceLocator.Bind<IMicrophone>().To(microphone_);
    }

    [SetUp]
    public void Setup() {
      voipThreadedMicrophoneInput_ = CreateVoipThreadedMicInput();
    }

    [OneTimeTearDown]
    public void Cleanup() {
      ServiceLocator.Clear();
    }

    [Test]
    public void TestMicInputListenerWithDefaultChunkSize() {
      var listener = Substitute.For<Action<float[]>>();
      
      voipThreadedMicrophoneInput_.AddMicInputListener(listener, DefaultChunkSizeMs);

      // The mic input callback is being called with a frame of length 2.5 * 20ms = 50 ms (or 2.5 * 960 = 2400 samples)
      voipThreadedMicrophoneInput_.ProcessSamplesFromBackgroundThread(PcmInput, (int)(2.5 * DefaultChunkSize));

      // We expect the listener to be called with 2 chunks of size 20 ms each, remain 10 ms in the chunker internal buffer
      listener.Received().Invoke(Arg.Is<float[]>(arr => arr.Length == 2 * DefaultChunkSize));

      // The mic input callback is then being called with a frame of length 0.8 * 20ms = 16 ms (or 0.8 * 960 = 768 samples)
      voipThreadedMicrophoneInput_.ProcessSamplesFromBackgroundThread(PcmInput, (int)(0.8 * DefaultChunkSize));

      // We expect the listener to be called with the previously remaining 10 ms samples plus the first
      // 10 ms samples from the new input, remain 0.3 * 20 ms = 6 ms in  the chunker internal buffer
      listener.Received().Invoke(Arg.Is<float[]>(arr => arr.Length == DefaultChunkSize));
    }

    [Test]
    public void TestMicInputMultipleListenerWithDifferentChunkSizes() {
      var listener5Ms = Substitute.For<Action<float[]>>();
      var listener10Ms = Substitute.For<Action<float[]>>();

      var chunkSize5Ms = VoipSamplesChunker.MsToChunkSize(5);
      var chunkSize10Ms = VoipSamplesChunker.MsToChunkSize(10);

      voipThreadedMicrophoneInput_.AddMicInputListener(listener5Ms, 5);
      voipThreadedMicrophoneInput_.AddMicInputListener(listener10Ms, 10);

      var pcmInput = new float[5 * chunkSize10Ms]; //Buffer that holds the pcm data

      // The mic input callback is being called with a frame of length 5 ms
      voipThreadedMicrophoneInput_.ProcessSamplesFromBackgroundThread(pcmInput, chunkSize5Ms);
      // The 5ms listener is invoked the first time because it corresponds to its chunk size
      listener5Ms.Received().Invoke(Arg.Is<float[]>(arr => arr.Length == chunkSize5Ms));
      // The 10ms listener is not invoked the first time because it hasn't reached the limit to process its chunk size yet
      listener10Ms.DidNotReceive().Invoke(Arg.Any<float[]>());

      // The mic input callback is being called with a frame of length 5 ms again
      voipThreadedMicrophoneInput_.ProcessSamplesFromBackgroundThread(pcmInput, chunkSize5Ms);
      // The 5ms listener is invoked the second time because it still corresponds to its chunk size
      listener5Ms.Received().Invoke(Arg.Is<float[]>(arr => arr.Length == chunkSize5Ms));
      // The 10ms listener is invoked because its aggregated state now contains 10ms of data
      listener10Ms.Received().Invoke(Arg.Is<float[]>(arr => arr.Length == chunkSize10Ms));
    }

    [Test]
    public void TestAddSameListenerMultipleTimes() {
      var listener = Substitute.For<Action<float[]>>();

      // Add the listener twice
      voipThreadedMicrophoneInput_.AddMicInputListener(listener, DefaultChunkSizeMs);
      voipThreadedMicrophoneInput_.AddMicInputListener(listener, DefaultChunkSizeMs);

      voipThreadedMicrophoneInput_.ProcessSamplesFromBackgroundThread(PcmInput, DefaultChunkSize);

      // Check that it has only been called once
      listener.Received(1).Invoke(Arg.Is<float[]>(arr => arr.Length == DefaultChunkSize));
    }

    [Test]
    public void TestRemoveNonExistentListener() {
      var listener = Substitute.For<Action<float[]>>();

      voipThreadedMicrophoneInput_.RemoveMicInputListener(listener);
    }
    
    [Test]
    public void TestAddRemoveSameListenerAddedWithMultipleChunkSizes() {
      var listener = Substitute.For<Action<float[]>>();

      // Add the listener twice with different chunk sizes
      voipThreadedMicrophoneInput_.AddMicInputListener(listener, DefaultChunkSizeMs);
      voipThreadedMicrophoneInput_.AddMicInputListener(listener, 2 * DefaultChunkSizeMs);
      
      // Call the mic input callback
      voipThreadedMicrophoneInput_.ProcessSamplesFromBackgroundThread(PcmInput, 2 * DefaultChunkSize);
      listener.Received(2).Invoke(Arg.Is<float[]>(arr => arr.Length ==  2 * DefaultChunkSize));

      // Remove it, it should remove all references over all chunk sizes
      voipThreadedMicrophoneInput_.RemoveMicInputListener(listener);

      listener.ClearReceivedCalls();
      // Call the mic input callback again
      voipThreadedMicrophoneInput_.ProcessSamplesFromBackgroundThread(PcmInput, DefaultChunkSize);

      // Check that the listener has not been called
      listener.DidNotReceive().Invoke(Arg.Any<float[]>());
    }

    [Test]
    public void TestAddListenerBypassMuteState() {
      var listenerBypassMuted = Substitute.For<Action<float[]>>();
      var listenerNoBypassMuted = Substitute.For<Action<float[]>>();

      voipThreadedMicrophoneInput_.AddMicInputListener(listenerBypassMuted, DefaultChunkSizeMs, true);
      voipThreadedMicrophoneInput_.AddMicInputListener(listenerNoBypassMuted, DefaultChunkSizeMs, false);

      voipThreadedMicrophoneInput_.SetMuted(true);

      voipThreadedMicrophoneInput_.ProcessSamplesFromBackgroundThread(PcmInput, DefaultChunkSize);

      listenerBypassMuted.Received(1).Invoke(Arg.Is<float[]>(arr => arr.Length == DefaultChunkSize));
      listenerNoBypassMuted.DidNotReceive().Invoke(Arg.Any<float[]>());
    }

    private VoipThreadedMicrophoneInput CreateVoipThreadedMicInput() {
      var voipThreadedMicrophoneInput = new VoipThreadedMicrophoneInput(microphone_, new MWSRStaticLockFreeQueue<float[]>(960), dispatcher_);
      voipThreadedMicrophoneInput.SetSkipUpdate(false);

      return voipThreadedMicrophoneInput;
    }
  }
}
