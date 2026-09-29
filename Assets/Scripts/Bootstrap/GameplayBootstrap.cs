using UnityEngine;
using UnityEngine.EventSystems;
using DancingBallOnBeats.Audio;
using DancingBallOnBeats.Core;
using DancingBallOnBeats.Data;
using DancingBallOnBeats.Gameplay;
using DancingBallOnBeats.UI;
using DancingBallOnBeats.Ads;

namespace DancingBallOnBeats.Bootstrap
{
    /// <summary>
    /// Entry point for the Gameplay scene. Attach this to a single empty GameObject in the
    /// scene (see Assets/Scenes/Gameplay.unity). Reads the song chosen in Song Select
    /// (GameSession.SelectedSongId), builds a matching LevelData, and assembles the entire
    /// play area (lighting, lane track, ball, camera, obstacle spawner, beat manager, HUD)
    /// purely at runtime — no imported models/prefabs required.
    /// </summary>
    public class GameplayBootstrap : MonoBehaviour
    {
        [Header("Fallback if no song was selected (e.g. testing this scene directly)")]
        [SerializeField] private string fallbackSongId = "real_backbeat";

        private void Awake()
        {
            EnsureEventSystem();
            EnsureAdsManager();

            SongData song = ResolveSelectedSong();
            LevelData level = BuildLevelForSong(song);

            SetupLighting();

            var laneTrackGO = new GameObject("LaneTrack");
            var laneTrack = laneTrackGO.AddComponent<LaneTrack>();
            laneTrack.Configure(level.laneCount);

            GameObject ball = BuildBall(laneTrack);
            var ballController = ball.GetComponent<BallController>();
            ballController.Initialize(laneTrack);

            laneTrack.BeginTracking(ball.transform);

            Camera cam = BuildCamera(ball.transform);

            var beatManagerGO = new GameObject("BeatManager");
            beatManagerGO.AddComponent<BeatManager>();

            var spawnerGO = new GameObject("ObstacleSpawner");
            var spawner = spawnerGO.AddComponent<ObstacleSpawner>();
            spawner.Initialize(laneTrack, ball.transform, level, seed: song.songId.GetHashCode());

            var gameManagerGO = new GameObject("GameManager");
            var gameManager = gameManagerGO.AddComponent<GameManager>();
            gameManager.Initialize(song, level, ballController, spawner);

            Canvas canvas = UIFactory.CreateCanvas("GameplayCanvas", out GameObject canvasRoot);
            canvasRoot.transform.SetParent(transform, false);
            HUDController.Build(canvasRoot.transform);

            gameManager.StartRun();
        }

        private SongData ResolveSelectedSong()
        {
            var library = AudioTrackLibrary.BuildDefaultLibrary();
            string songId = !string.IsNullOrEmpty(GameSession.SelectedSongId) ? GameSession.SelectedSongId : fallbackSongId;
            SongData song = AudioTrackLibrary.FindById(library, songId);
            if (song == null)
            {
                Debug.LogWarning($"[GameplayBootstrap] Song id '{songId}' not found, falling back to first entry.");
                song = library[0];
            }
            return song;
        }

        private LevelData BuildLevelForSong(SongData song)
        {
            ObstaclePattern pattern;
            int laneCount;
            int beatsPerObstacle;
            float baseSpeed;

            switch (song.difficulty)
            {
                case 1:
                    pattern = ObstaclePattern.Classic;
                    laneCount = 3;
                    beatsPerObstacle = 2;
                    baseSpeed = 6f;
                    break;
                case 2:
                    pattern = ObstaclePattern.ZigZag;
                    laneCount = 3;
                    beatsPerObstacle = 1;
                    baseSpeed = 7f;
                    break;
                case 3:
                    pattern = ObstaclePattern.GapRun;
                    laneCount = 4;
                    beatsPerObstacle = 1;
                    baseSpeed = 8f;
                    break;
                default: // 4-5
                    pattern = ObstaclePattern.Chaos;
                    laneCount = 4;
                    beatsPerObstacle = 1;
                    baseSpeed = 9f;
                    break;
            }

            return LevelData.CreateRuntime(song.songId, laneCount, pattern, beatsPerObstacle, startingLives: 3, baseScrollSpeed: baseSpeed);
        }

        private void SetupLighting()
        {
            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.color = new Color(1f, 0.98f, 0.92f);
            lightGO.transform.rotation = Quaternion.Euler(55f, -30f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.18f, 0.18f, 0.24f);
        }

        private GameObject BuildBall(LaneTrack laneTrack)
        {
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Ball";
            ball.transform.position = new Vector3(laneTrack.GetLaneX(laneTrack.MiddleLaneIndex), 0.5f, 0f);
            ball.transform.localScale = Vector3.one * 0.9f;

            var renderer = ball.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.3f, 0.85f, 1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.1f, 0.4f, 0.5f));
            renderer.material = mat;

            var rb = ball.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.linearDamping = 0.5f;

            // Use a trigger collider for obstacle detection (BallController.OnTriggerEnter),
            // plus a slightly smaller physical collider is unnecessary here since the ground
            // uses a non-trigger collider and the sphere collider handles both ground contact
            // (isTrigger=false) - obstacles create their own trigger colliders instead.
            var col = ball.GetComponent<SphereCollider>();
            col.isTrigger = false;

            ball.AddComponent<BallController>();
            ball.tag = "Player";

            return ball;
        }

        private Camera BuildCamera(Transform ballTransform)
        {
            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.03f, 0.03f, 0.06f);
            cam.fieldOfView = 60f;

            camGO.AddComponent<AudioListener>();

            var follow = camGO.AddComponent<CameraFollow>();
            follow.SetTarget(ballTransform);

            return cam;
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private void EnsureAdsManager()
        {
            if (AdsManager.Instance != null) return;
            var go = new GameObject("AdsManager");
            go.AddComponent<AdsManager>().Initialize();
        }
    }
}
