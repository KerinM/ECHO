using System.Collections.Generic;
using UnityEngine;

namespace Echo.EditorTools
{
    internal static class ForestScatter
    {
        internal const float InnerRadius = 25f;
        internal const float OuterRadius = 37f;

        internal readonly struct Segment
        {
            public readonly Vector2 Start;
            public readonly Vector2 End;

            public Segment(Vector2 start, Vector2 end)
            {
                Start = start;
                End = end;
            }
        }

        internal readonly struct Footprint
        {
            public readonly Vector2 Position;
            public readonly float Radius;

            public Footprint(Vector2 position, float radius)
            {
                Position = position;
                Radius = radius;
            }
        }

        internal struct Rng
        {
            private uint state;

            public Rng(uint seed)
            {
                state = seed == 0u ? 0x9E3779B9u : seed;
            }

            public uint NextUInt()
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
                return state;
            }

            public float Next01()
            {
                return (NextUInt() & 0x00FFFFFFu) / 16777216f;
            }

            public float Range(float min, float max)
            {
                return min + (max - min) * Next01();
            }
        }

        internal static Vector2 SampleAnnulus(ref Rng rng, float innerRadius, float outerRadius)
        {
            float angle = rng.Range(0f, Mathf.PI * 2f);
            float radius = Mathf.Sqrt(rng.Range(innerRadius * innerRadius, outerRadius * outerRadius));
            return new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
        }

        internal static void AddPolylineSegments(List<Segment> segments, Vector2[] localPoints, Transform region)
        {
            if (segments == null || localPoints == null || localPoints.Length < 2)
            {
                return;
            }

            for (int i = 1; i < localPoints.Length; i++)
            {
                Vector2 previous = localPoints[i - 1];
                Vector2 current = localPoints[i];
                Vector3 start = region.TransformPoint(new Vector3(previous.x, 0f, previous.y));
                Vector3 end = region.TransformPoint(new Vector3(current.x, 0f, current.y));
                segments.Add(new Segment(new Vector2(start.x, start.z), new Vector2(end.x, end.z)));
            }
        }

        internal static bool IsNearPath(Vector2 point, List<Segment> segments, float clearance)
        {
            if (segments == null || segments.Count == 0)
            {
                return false;
            }

            float clearanceSquared = clearance * clearance;
            for (int i = 0; i < segments.Count; i++)
            {
                Segment segment = segments[i];
                if (DistanceSquaredToSegment(point, segment.Start, segment.End) < clearanceSquared)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool OverlapsAny(Vector2 point, float radius, List<Footprint> footprints)
        {
            if (footprints == null || footprints.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < footprints.Count; i++)
            {
                Footprint footprint = footprints[i];
                float minimum = radius + footprint.Radius;
                if ((point - footprint.Position).sqrMagnitude < minimum * minimum)
                {
                    return true;
                }
            }

            return false;
        }

        private static float DistanceSquaredToSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 direction = end - start;
            float lengthSquared = direction.sqrMagnitude;
            if (lengthSquared <= 0.000001f)
            {
                return (point - start).sqrMagnitude;
            }

            float t = Mathf.Clamp01(Vector2.Dot(point - start, direction) / lengthSquared);
            Vector2 closest = start + direction * t;
            return (point - closest).sqrMagnitude;
        }
    }
}
