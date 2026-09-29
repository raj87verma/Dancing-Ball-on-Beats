using System;
using UnityEngine;
using DancingBallOnBeats.Audio;

namespace DancingBallOnBeats.Gameplay
{
    /// <summary>
    /// Drives the player's marble: swipe/tap left-right to change lanes, tap/swipe up to jump,
    /// both snapped to a smooth beat-synced motion so the ball visually "bounces" with the
    /// music (matching the reference apps' core feel). Reports hit judgement for scoring via
    /// events, and forwards collisions with Obstacle components to whoever is listening
    /// (GameManager) rather than handling game-over logic itself.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BallController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float laneChangeSpeed = 14f;
        [SerializeField] private float jumpForce = 6.5f;
        [SerializeField] private float bounceScaleAmount = 0.15f;
        [SerializeField] private float bounceScaleSpeed = 10f;

        [Header("Input")]
        [SerializeField] private float swipeThresholdPixels = 40f;

        [Header("Timing")]
        [Tooltip("Seconds of tolerance around a beat to count as 'Perfect'.")]
        [SerializeField] private float perfectWindowSeconds = 0.08f;
        [Tooltip("Seconds of tolerance around a beat to count as 'Good' (must be wider than Perfect).")]
        [SerializeField] private float goodWindowSeconds = 0.16f;

        public event Action<HitJudgement, Obstacle> OnObstacleResolved;
        public event Action OnJumped;
        public event Action<int> OnLaneChanged;

        private Rigidbody _rigidbody;
        private LaneTrack _laneTrack;
        private int _currentLaneIndex;
        private float _targetX;
        private bool _isGrounded = true;
        private Vector3 _baseScale;
        private Vector2 _touchStartPos;
        private bool _touchActive;

        public bool InputEnabled { get; set; } = true;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.freezeRotation = true;
            _baseScale = transform.localScale;
        }

        public void Initialize(LaneTrack laneTrack)
        {
            _laneTrack = laneTrack;
            _currentLaneIndex = laneTrack.MiddleLaneIndex;
            _targetX = laneTrack.GetLaneX(_currentLaneIndex);
            var pos = transform.position;
            transform.position = new Vector3(_targetX, pos.y, pos.z);
        }

        private void Update()
        {
            if (InputEnabled)
            {
                HandleInput();
            }

            // Smoothly glide toward the target lane X position.
            Vector3 current = transform.position;
            float newX = Mathf.MoveTowards(current.x, _targetX, laneChangeSpeed * Time.deltaTime);
            transform.position = new Vector3(newX, current.y, current.z + 0f);

            // Beat-synced squash/stretch pulse purely for visual feel.
            if (BeatManager.Instance != null && BeatManager.Instance.IsPlaying)
            {
                float progress = BeatManager.Instance.BeatProgress01;
                float pulse = 1f - Mathf.Abs(progress - 0f) * 2f; // peak right on the beat
                pulse = Mathf.Clamp01(pulse);
                float scaleY = 1f - bounceScaleAmount * pulse;
                float scaleXZ = 1f + (bounceScaleAmount * 0.5f) * pulse;
                transform.localScale = new Vector3(_baseScale.x * scaleXZ, _baseScale.y * scaleY, _baseScale.z * scaleXZ);
            }
        }

        private void HandleInput()
        {
            // Keyboard support (useful for quick testing outside a touch device).
            if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveLane(-1);
            if (Input.GetKeyDown(KeyCode.RightArrow)) MoveLane(1);
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) Jump();

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        _touchStartPos = touch.position;
                        _touchActive = true;
                        break;
                    case TouchPhase.Moved:
                    case TouchPhase.Ended:
                        if (_touchActive)
                        {
                            Vector2 delta = touch.position - _touchStartPos;
                            if (Mathf.Abs(delta.x) > swipeThresholdPixels && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                            {
                                MoveLane(delta.x > 0 ? 1 : -1);
                                _touchActive = false;
                            }
                            else if (delta.y > swipeThresholdPixels && Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                            {
                                Jump();
                                _touchActive = false;
                            }
                        }
                        if (touch.phase == TouchPhase.Ended) _touchActive = false;
                        break;
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                // Basic click-to-jump support for desktop testing.
                Jump();
            }
        }

        public void MoveLane(int direction)
        {
            if (_laneTrack == null) return;
            int newLane = Mathf.Clamp(_currentLaneIndex + direction, 0, _laneTrack.LaneCount - 1);
            if (newLane == _currentLaneIndex) return;

            _currentLaneIndex = newLane;
            _targetX = _laneTrack.GetLaneX(_currentLaneIndex);
            OnLaneChanged?.Invoke(_currentLaneIndex);
        }

        public void Jump()
        {
            if (!_isGrounded) return;
            _isGrounded = false;
            _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            OnJumped?.Invoke();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground") || collision.gameObject.name == "FloorSegment")
            {
                _isGrounded = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var obstacle = other.GetComponent<Obstacle>();
            if (obstacle == null || obstacle.Consumed) return;

            HitJudgement judgement = EvaluateTiming();

            bool laneMatches = obstacle.LaneIndex == _currentLaneIndex;
            bool resolvedSafely;

            switch (obstacle.Type)
            {
                case ObstacleType.LaneBlock:
                    // Safe only if the ball is NOT in the blocked lane.
                    resolvedSafely = !laneMatches;
                    break;
                case ObstacleType.JumpGap:
                    // Safe only if airborne (jumped) when passing through.
                    resolvedSafely = !_isGrounded;
                    break;
                case ObstacleType.BonusNote:
                    resolvedSafely = true; // always "safe", timing just affects bonus size
                    break;
                default:
                    resolvedSafely = true;
                    break;
            }

            obstacle.MarkConsumed();
            HitJudgement finalJudgement = resolvedSafely ? judgement : HitJudgement.Miss;
            OnObstacleResolved?.Invoke(finalJudgement, obstacle);
        }

        private HitJudgement EvaluateTiming()
        {
            if (BeatManager.Instance == null || !BeatManager.Instance.IsPlaying)
                return HitJudgement.Good;

            float progress = BeatManager.Instance.BeatProgress01;
            float secondsPerBeat = BeatManager.Instance.SecondsPerBeat;
            float secondsFromNearestBeat = Mathf.Min(progress, 1f - progress) * secondsPerBeat;

            if (secondsFromNearestBeat <= perfectWindowSeconds) return HitJudgement.Perfect;
            if (secondsFromNearestBeat <= goodWindowSeconds) return HitJudgement.Good;
            return HitJudgement.Good; // still passable outside tight windows; only obstacle logic decides true Miss
        }

        public int CurrentLaneIndex => _currentLaneIndex;
        public bool IsGrounded => _isGrounded;
    }
}
