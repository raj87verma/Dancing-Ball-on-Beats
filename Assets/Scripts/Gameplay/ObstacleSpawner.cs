using System.Collections.Generic;
using UnityEngine;
using DancingBallOnBeats.Audio;
using DancingBallOnBeats.Data;

namespace DancingBallOnBeats.Gameplay
{
    /// <summary>
    /// Listens to BeatManager.OnBeat and spawns obstacles ahead of the ball according to the
    /// active LevelData's pattern. Obstacles are simple primitives built at runtime (colored
    /// cubes/arches) so the whole level needs zero imported 3D models.
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        [SerializeField] private Material obstacleMaterial;
        [SerializeField] private float spawnDistanceAhead = 40f;
        [SerializeField] private float despawnDistanceBehind = 8f;

        private LaneTrack _laneTrack;
        private Transform _ballTransform;
        private LevelData _levelData;
        private float _currentScrollSpeed;
        private int _beatsSinceLastSpawn;
        private System.Random _rng;
        private readonly List<Obstacle> _active = new List<Obstacle>();

        public void Initialize(LaneTrack laneTrack, Transform ballTransform, LevelData levelData, int seed)
        {
            _laneTrack = laneTrack;
            _ballTransform = ballTransform;
            _levelData = levelData;
            _currentScrollSpeed = levelData.baseScrollSpeed;
            _rng = new System.Random(seed);
            _beatsSinceLastSpawn = 0;

            if (BeatManager.Instance != null)
            {
                BeatManager.Instance.OnBeat += HandleBeat;
            }
        }

        private void OnDestroy()
        {
            if (BeatManager.Instance != null)
            {
                BeatManager.Instance.OnBeat -= HandleBeat;
            }
        }

        private void HandleBeat(int beatIndex)
        {
            _currentScrollSpeed += _levelData.speedRampPerBeat;
            SyncSpeedToActiveObstacles();

            _beatsSinceLastSpawn++;
            if (_beatsSinceLastSpawn < _levelData.beatsPerObstacle) return;
            _beatsSinceLastSpawn = 0;

            SpawnForPattern(beatIndex);
        }

        private void SyncSpeedToActiveObstacles()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (_active[i] == null) { _active.RemoveAt(i); continue; }
                _active[i].SetScrollSpeed(_currentScrollSpeed);
            }
        }

        private void SpawnForPattern(int beatIndex)
        {
            switch (_levelData.pattern)
            {
                case ObstaclePattern.Classic:
                    SpawnLaneBlock(RandomLane());
                    break;

                case ObstaclePattern.ZigZag:
                    // Alternate forcing the ball left/right by blocking the opposite side each time.
                    int lane = (beatIndex / Mathf.Max(1, _levelData.beatsPerObstacle)) % 2 == 0 ? 0 : _laneTrack.LaneCount - 1;
                    SpawnLaneBlock(lane);
                    break;

                case ObstaclePattern.GapRun:
                    if (_rng.Next(0, 3) == 0)
                        SpawnJumpGap();
                    else
                        SpawnLaneBlock(RandomLane());
                    break;

                case ObstaclePattern.Chaos:
                    int roll = _rng.Next(0, 4);
                    if (roll == 0) SpawnJumpGap();
                    else if (roll == 1) SpawnBonusNote(RandomLane());
                    else SpawnLaneBlock(RandomLane());
                    break;
            }
        }

        private int RandomLane() => _rng.Next(0, _laneTrack.LaneCount);

        private float SpawnZ => _ballTransform != null ? _ballTransform.position.z + spawnDistanceAhead : spawnDistanceAhead;
        private float DespawnZ => _ballTransform != null ? _ballTransform.position.z - despawnDistanceBehind : -despawnDistanceBehind;

        private void SpawnLaneBlock(int laneIndex)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Obstacle_LaneBlock";
            go.transform.SetParent(transform);
            float x = _laneTrack.GetLaneX(laneIndex);
            go.transform.position = new Vector3(x, 0.5f, SpawnZ);
            go.transform.localScale = new Vector3(_laneTrack.LaneWidth * 0.85f, 1.4f, 0.8f);
            ApplyMaterial(go, new Color(0.95f, 0.25f, 0.3f));
            SetupColliderAsTrigger(go);

            var obstacle = go.AddComponent<Obstacle>();
            obstacle.Initialize(ObstacleType.LaneBlock, laneIndex, _currentScrollSpeed, DespawnZ);
            _active.Add(obstacle);
        }

        private void SpawnJumpGap()
        {
            // A JumpGap spans all lanes at once, so LaneIndex is irrelevant for its safety check.
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Obstacle_JumpGap";
            go.transform.SetParent(transform);
            go.transform.position = new Vector3(0f, 0.25f, SpawnZ);
            go.transform.localScale = new Vector3(_laneTrack.TotalWidth + 0.5f, 0.5f, 0.8f);
            ApplyMaterial(go, new Color(1f, 0.6f, 0.1f));
            SetupColliderAsTrigger(go);

            var obstacle = go.AddComponent<Obstacle>();
            obstacle.Initialize(ObstacleType.JumpGap, -1, _currentScrollSpeed, DespawnZ);
            _active.Add(obstacle);
        }

        private void SpawnBonusNote(int laneIndex)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Obstacle_BonusNote";
            go.transform.SetParent(transform);
            float x = _laneTrack.GetLaneX(laneIndex);
            go.transform.position = new Vector3(x, 0.6f, SpawnZ);
            go.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            ApplyMaterial(go, new Color(1f, 0.95f, 0.3f));
            SetupColliderAsTrigger(go);

            var obstacle = go.AddComponent<Obstacle>();
            obstacle.Initialize(ObstacleType.BonusNote, laneIndex, _currentScrollSpeed, DespawnZ);
            _active.Add(obstacle);
        }

        private void ApplyMaterial(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            var mat = obstacleMaterial != null ? new Material(obstacleMaterial) : new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
        }

        private void SetupColliderAsTrigger(GameObject go)
        {
            var collider = go.GetComponent<Collider>();
            if (collider != null) collider.isTrigger = true;
        }
    }
}
