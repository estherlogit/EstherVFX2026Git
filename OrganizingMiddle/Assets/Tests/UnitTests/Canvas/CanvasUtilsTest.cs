// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Canvas;
using NUnit.Framework;
using UnityEngine;

namespace Facebook.Workrooms.Tests.UnitTests {
  [OnCall(OnCallName.workrooms_creative_collaborations)]
  public class CanvasUtilsTest {
    // Const value normally provide via configuration file, hardcoded here just for tests
    const float MAX_DIMENSION = 512f;

    [Test]
    public void TestImageAnnotationResolutions() {
      // If both axis < max dimension, annotation resolution scaled up to clamp larger axis to max dimension
      AssertImageAnnotationResolutions(
        elemSize: new Vector2(MAX_DIMENSION / 2, MAX_DIMENSION / 4),
        expected: new Vector2(MAX_DIMENSION, MAX_DIMENSION / 2)
      );

      AssertImageAnnotationResolutions(
        elemSize: new Vector2(MAX_DIMENSION / 4, MAX_DIMENSION / 2),
        expected: new Vector2(MAX_DIMENSION / 2, MAX_DIMENSION)
      );

      // No change to annotation resolution if one axis = max size & other <= max size
      AssertImageAnnotationResolutions(
        elemSize: new Vector2(MAX_DIMENSION / 2, MAX_DIMENSION),
        expected: new Vector2(MAX_DIMENSION / 2, MAX_DIMENSION)
      );

      AssertImageAnnotationResolutions(
        elemSize: new Vector2(MAX_DIMENSION, MAX_DIMENSION / 2),
        expected: new Vector2(MAX_DIMENSION, MAX_DIMENSION / 2)
      );

      AssertImageAnnotationResolutions(
        elemSize: new Vector2(MAX_DIMENSION, MAX_DIMENSION),
        expected: new Vector2(MAX_DIMENSION, MAX_DIMENSION)
      );

      // Exceeding the max dimension => annotation resolution scaled down with larger aspect clamped to max dimension
      AssertImageAnnotationResolutions(
        elemSize: new Vector2(MAX_DIMENSION * 2, MAX_DIMENSION),
        expected: new Vector2(MAX_DIMENSION, MAX_DIMENSION / 2)
      );

      AssertImageAnnotationResolutions(
        elemSize: new Vector2(MAX_DIMENSION, MAX_DIMENSION * 2),
        expected: new Vector2(MAX_DIMENSION / 2, MAX_DIMENSION)
      );
    }

    private void AssertImageAnnotationResolutions(Vector2 elemSize, Vector2 expected) {
      var annotationResolution = CanvasConfig.GetImageAnnotationResolution(elemSize, MAX_DIMENSION);
      Assert.AreEqual(
        expected,
        annotationResolution,
        "Annotation resolution not set as expected. element size: {0}. annotation resolution: {1}. expected resolution: {2}",
        elemSize,
        annotationResolution,
        expected
      );
    }
  }
}
