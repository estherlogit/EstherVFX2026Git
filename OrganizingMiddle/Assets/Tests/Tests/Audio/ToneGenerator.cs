using System;

namespace Facebook.Workrooms.Tests.Tests.Audio {
  internal class ToneGenerator {

    private float[] samples;
    private int samplePointer_;

    public ToneGenerator(int freq, int samplingRate) {
      // Nb of samples for 1 sec
      samples = new float[samplingRate];
      samplePointer_ = 0;
      for (int i = 0; i < samplingRate; i++) {
        samples[i] = (float) (Math.Sin((i * 2 * Math.PI * freq) / samplingRate));
      }
    }

    public float[] GetNextSamples(int numberOfSamples) {
      var ret = new float[numberOfSamples];
      for (int i = 0; i < numberOfSamples; i++) {
        ret[i] = samples[samplePointer_];
        samplePointer_ = (samplePointer_+1)% samples.Length;
      }
      return ret;
    }
  }
}
