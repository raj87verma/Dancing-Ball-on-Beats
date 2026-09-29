using System.Collections.Generic;
using UnityEngine;

namespace DancingBallOnBeats.Gameplay
{
    /// <summary>
    /// Defines the lane layout (how many lanes, how wide) and continuously builds/recycles
    /// simple procedural floor segments ahead of the ball so the track appears endless without
    /// needing any hand-authored 3D level geometry or imported models.
    /// </summary>
    public class LaneTrack : MonoBehaviour
    {
        [Header("Lane Layout")]
        [SerializeField] private int laneCount = 3;
        [SerializeField] private float laneWidth = 2.2f;

        [Header("Floor Generation")]
        [SerializeField] private float segmentLength = 10f;
        [SerializeField] private int segmentsAhead = 6;
        [SerializeField] private Material floorMaterial;
        [SerializeField] private Color floorColorA = new Color(0.12f, 0.12f, 0.18f);
        [SerializeField] private Color floorColorB = new Color(0.10f, 0.10f, 0.15f);

        private readonly List<GameObject> _segments = new List<GameObject>();
        private float _nextSpawnZ;
        private Transform _followTarget;

        public int LaneCount => laneCount;
        public float LaneWidth => laneWidth;
        public float TotalWidth => laneCount * laneWidth;

        public void Configure(int newLaneCount, float newLaneWidth = -1f)
        {
            laneCount = Mathf.Clamp(newLaneCount, 2, 5);
            if (newLaneWidth > 0f) laneWidth = newLaneWidth;
        }

        /// <summary>Center X position of a given lane index (0-based, left to right).</summary>
        public float GetLaneX(int laneIndex)
        {
            laneIndex = Mathf.Clamp(laneIndex, 0, laneCount - 1);
            float totalWidth = TotalWidth;
            float leftEdge = -totalWidth / 2f + laneWidth / 2f;
            return leftEdge + laneIndex * laneWidth;
        }

        public int MiddleLaneIndex => laneCount / 2;

        public void BeginTracking(Transform target)
        {
            _followTarget = target;
            _nextSpawnZ = target != null ? target.position.z : 0f;

            foreach (var seg in _segments) Destroy(seg);
            _segments.Clear();

            for (int i = 0; i < segmentsAhead; i++)
            {
                SpawnSegment();
            }
        }

        private void Update()
        {
            if (_followTarget == null) return;

            // Spawn new segments as the ball advances.
            while (_nextSpawnZ < _followTarget.position.z + segmentsAhead * segmentLength)
            {
                SpawnSegment();
            }

            // Recycle segments that have fallen far enough behind the ball.
            for (int i = _segments.Count - 1; i >= 0; i--)
            {
                if (_segments[i].transform.position.z < _followTarget.position.z - segmentLength * 2f)
                {
                    Destroy(_segments[i]);
                    _segments.RemoveAt(i);
                }
            }
        }

        private void SpawnSegment()
        {
            var segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.name = "FloorSegment";
            segment.transform.SetParent(transform);
            segment.transform.position = new Vector3(0f, -0.5f, _nextSpawnZ + segmentLength / 2f);
            segment.transform.localScale = new Vector3(TotalWidth + 0.5f, 1f, segmentLength);

            var renderer = segment.GetComponent<Renderer>();
            var mat = floorMaterial != null ? new Material(floorMaterial) : new Material(Shader.Find("Standard"));
            bool alt = (_segments.Count % 2) == 0;
            mat.color = alt ? floorColorA : floorColorB;
            renderer.material = mat;

            var collider = segment.GetComponent<Collider>();
            if (collider != null) collider.isTrigger = false;

            _segments.Add(segment);
            _nextSpawnZ += segmentLength;
        }
    }
}
