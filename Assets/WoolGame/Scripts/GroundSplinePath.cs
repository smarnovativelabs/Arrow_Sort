using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace WoolGame
{
    // Lightweight path wrapper used by the authored scene. Provides a simple
    // length-based API (Length, Position, Tangent) on top of a SplineContainer.
    // This implementation samples the underlying spline to produce stable
    // length and position queries without depending on editor-only utilities.
    [RequireComponent(typeof(SplineContainer))]
    public sealed class GroundSplinePath : MonoBehaviour
    {
        public SplineContainer Container;

        // number of samples used for length/position approximation
        [SerializeField, Min(8)] int samples = 256;

        float[] cumulative;
        Vector3[] points;
        float totalLength;

        void Reset() => Container = GetComponent<SplineContainer>();

        void Awake() { if (Container == null) Container = GetComponent<SplineContainer>(); Rebuild(); }

        public void Rebuild()
        {
            if (Container == null) Container = GetComponent<SplineContainer>();
            BuildSamples();
        }
        public void UseBaked(Vector3[] pathPoints,float[] pathDistances)
        {
            points=pathPoints;cumulative=pathDistances;
            totalLength=cumulative.Length>0?cumulative[cumulative.Length-1]:0;
        }

        void BuildSamples()
        {
            if (Container == null || Container.Splines == null)
            {
                points = Array.Empty<Vector3>(); cumulative = Array.Empty<float>(); totalLength = 0; return;
            }

            int n = Math.Max(8, samples);
            points = new Vector3[n + 1];
            cumulative = new float[n + 1];

            Vector3 prev = Container.EvaluatePosition(0f);
            points[0] = prev; cumulative[0] = 0f; totalLength = 0f;
            for (int i = 1; i <= n; i++)
            {
                float t = (float)i / n;
                Vector3 p = Container.EvaluatePosition(t);
                points[i] = p;
                totalLength += Vector3.Distance(prev, p);
                cumulative[i] = totalLength;
                prev = p;
            }
        }

        // Total path length in world units (approximated by sampling)
        public float Length => totalLength;

        // Get world-space position at a given distance along the path. Distance
        // is clamped to [0,Length]. If the spline is empty, returns Vector3.zero.
        public Vector3 Position(float distance)
        {
            if (points == null || points.Length == 0) return Vector3.zero;
            if (totalLength <= 0f) return points[0];
            float d = Mathf.Clamp(distance, 0f, totalLength);
            // find segment
            int idx = Array.BinarySearch(cumulative, d);
            if (idx >= 0) return points[idx];
            idx = ~idx;
            if (idx <= 0) return points[0];
            if (idx >= points.Length) return points[points.Length - 1];
            float segStart = cumulative[idx - 1];
            float segEnd = cumulative[idx];
            float t = (d - segStart) / Math.Max(1e-6f, segEnd - segStart);
            return Vector3.Lerp(points[idx - 1], points[idx], t);
        }

        // Approximate tangent (direction) at a given distance using neighboring samples.
        public Vector3 Tangent(float distance)
        {
            if (points == null || points.Length == 0) return Vector3.right;
            float d = Mathf.Clamp(distance, 0f, totalLength);
            // pick a small offset in world distance to compute finite difference
            float eps = Math.Max(1e-3f, totalLength * 0.002f);
            Vector3 a = Position(d - eps);
            Vector3 b = Position(d + eps);
            var dir = (b - a).normalized;
            if (dir.sqrMagnitude <= 0f) return Vector3.right;
            return dir;
        }
    }
}
