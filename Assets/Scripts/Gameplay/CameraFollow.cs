using UnityEngine;

namespace DancingBallOnBeats.Gameplay
{
    /// <summary>
    /// Simple third-person chase camera: stays behind and above the ball with light smoothing,
    /// and always looks slightly ahead of it for a good sense of forward speed.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 4.5f, -6.5f);
        [SerializeField] private float positionSmoothTime = 0.12f;
        [SerializeField] private float lookAheadDistance = 4f;

        private Vector3 _velocity;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                transform.position = target.position + offset;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, positionSmoothTime);

            Vector3 lookTarget = target.position + Vector3.forward * lookAheadDistance;
            transform.LookAt(lookTarget);
        }
    }
}
