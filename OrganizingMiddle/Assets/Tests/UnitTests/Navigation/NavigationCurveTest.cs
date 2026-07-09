using System.Collections.Generic;
using System.Linq;
using Facebook.SocialVR.Core.OnCall;
using Facebook.Workrooms.Navigation;
using NUnit.Framework;
using UnityEngine;

namespace Facebook.Workrooms.Tests.UnitTests.Navigation {
  [OnCall(OnCallName.workrooms_core)]
  public class NavigationCurveTest {
    [OnCall(OnCallName.workrooms_core)]
    private class NavigationCurveRendererMock : INavigationCurveRenderer {
      public NavigationCurveRendererMock(int pointCount) {
        Points = new List<Vector3>(pointCount);
        for (var a = 0; a < pointCount; ++a) {
          Points.Add(new Vector3());
        }
      }

      public void SetPosition(int index, Vector3 position) {
        Debug.Assert(index < Points.Count);
        Points[index] = position;
      }

      public int PositionCount => Points.Count;

      public readonly List<Vector3> Points;
    }

    // construct a new random polygon, with Y axis non-negative
    private NavigationCurve.GuidePolygon MakeRandomPolygon() {
      var poly = new NavigationCurve.GuidePolygon {
        Origin = new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f)),
        Target = new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f)),
        OriginTangent = new Vector3(Random.Range(-3.0f, 3.0f), Random.Range(0.0f, 3.0f), Random.Range(-3.0f, 3.0f)),
        TargetTangent = new Vector3(Random.Range(-3.0f, 3.0f), Random.Range(0.0f, 3.0f), Random.Range(-3.0f, 3.0f))
      };

      return poly;
    }

    [Test]
    public void CurvePointsBoundsProperties() {
      // make this test deterministic
      Random.InitState(32166);

      for (var a = 0; a < 100; ++a) {
        // random polygon with non-negative Y axes
        var poly = MakeRandomPolygon();

        // make a mock line renderer to test the curve output
        var renderer = new NavigationCurveRendererMock(Random.Range(2, 50));

        // construct a curve out of this
        var curve = new NavigationCurve(renderer);
        curve.SetCurve(poly);

        // start and end of curve should match the polygon
        Assert.AreEqual(poly.Origin, renderer.Points.First());
        Assert.AreEqual(poly.Target, renderer.Points.Last());

        {
          // all the points in the curve should be inside the bounding box of the polygon
          var bounds = new Bounds();
          bounds.Encapsulate(poly.Origin);
          bounds.Encapsulate(poly.Target);
          bounds.Encapsulate(poly.Origin + poly.OriginTangent);
          bounds.Encapsulate(poly.Target + poly.TargetTangent);

          // grow the bounding box a tiny bit to account for accuracy problems
          bounds.extents *= 1.001f;

          foreach (var p in renderer.Points) {
            Assert.True(bounds.Contains(p), $"Bounds check failed - {p} not in {bounds.min} -- {bounds.max}");
          }
        }
      }
    }

    // construct a new random polygon, with Y axis non-negative, and values more representative to what we see in the app
    private NavigationCurve.GuidePolygon MakeNavCurvePolygon() {
      var poly = new NavigationCurve.GuidePolygon {
        Origin = new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(-0.5f, 0.5f), Random.Range(-10.0f, 10.0f)),
        Target = new Vector3(Random.Range(-10.0f, 10.0f), 0.0f, Random.Range(-10.0f, 10.0f)),
      };

      poly.OriginTangent = (poly.Target - poly.Origin) * 0.3f + new Vector3(0.0f, Random.Range(0.1f, 0.4f), 0.0f);
      poly.TargetTangent = new Vector3(0, (poly.Target - poly.Origin).magnitude * 0.3f, 0);

      return poly;
    }

    [Test]
    public void CurveSegmentProperties() {
      // make this test deterministic
      Random.InitState(3694722);

      for (var a = 0; a < 100; ++a) {
        // random polygon with non-negative Y axes
        var poly = MakeNavCurvePolygon();

        // make a mock line renderer to test the curve output
        var renderer = new NavigationCurveRendererMock(64);

        // construct a curve out of this
        var curve = new NavigationCurve(renderer);
        curve.SetCurve(poly);

        for (var i = 1; i < renderer.PositionCount - 1; ++i) {
          // the angle between neighbouring segments should be small-ish to keep the curve looking smooth
          var current = renderer.Points[i - 1] - renderer.Points[i];
          var next = renderer.Points[i] - renderer.Points[i + 1];

          Assert.Less(Vector3.Angle(current, next), 8.0f);
        }
      }
    }
  }
}
