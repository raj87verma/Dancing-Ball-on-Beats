using UnityEngine;

namespace DancingBallOnBeats.Gameplay
{
    /// <summary>What kind of interaction an obstacle requires from the player.</summary>
    public enum ObstacleType
    {
        /// <summary>Sits in one lane — the ball must be in a different lane when it arrives.</summary>
        LaneBlock,
        /// <summary>Spans the full track width — the ball must jump over it, timed to the beat.</summary>
        JumpGap,
        /// <summary>A collectible beat-note: no penalty for missing, but hitting it on-beat gives bonus score.</summary>
        BonusNote
    }

    /// <summary>
    /// A single obstacle/note instance on the track. Movement is driven externally by
    /// ObstacleSpawner (which sets scroll speed once, shared across all active obstacles) —
    /// each Obstacle just knows how to move itself backward each frame and report collisions.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Obstacle : MonoBehaviour
    {
        public ObstacleType Type { get; private set; }
        public int LaneIndex { get; private set; }
        public bool Consumed { get; private set; }

        private float _scrollSpeed;
        private float _despawnZ;

        public void Initialize(ObstacleType type, int laneIndex, float scrollSpeed, float despawnZ)
        {
            Type = type;
            LaneIndex = laneIndex;
            _scrollSpeed = scrollSpeed;
            _despawnZ = despawnZ;
            Consumed = false;
        }

        public void SetScrollSpeed(float speed)
        {
            _scrollSpeed = speed;
        }

        private void Update()
        {
            transform.position += Vector3.back * _scrollSpeed * Time.deltaTime;

            if (transform.position.z < _despawnZ)
            {
                Destroy(gameObject);
            }
        }

        public void MarkConsumed()
        {
            Consumed = true;
        }
    }
}
