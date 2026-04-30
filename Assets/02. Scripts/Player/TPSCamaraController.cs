using UnityEngine;
using UnityEngine.InputSystem;

namespace OneShot
{
    // TPS 카메라 — 로컬 전용 MonoBehaviour (네트워크 동기화 불필요)
    // PlayerController.Spawned()에서 HasInputAuthority인 경우에만 SetTarget() 호출
    public class TPSCameraController : MonoBehaviour
    {
        [Header("카메라 설정")]
        [SerializeField] private float   sensitivity     = 120f;
        [SerializeField] private float   minPitch        = -35f;
        [SerializeField] private float   maxPitch        = 60f;

        [Header("카메라 위치")]
        [SerializeField] private Vector3 offset          = new Vector3(0.5f, 1.2f, -4f);
        [SerializeField] private float   followSmoothness = 20f;

        private Transform _target;
        private float     _yaw;
        private float     _pitch;

        public void SetTarget(Transform target)
        {
            _target = target;
            if (target != null)
            {
                _yaw   = target.eulerAngles.y;
                _pitch = 0f;
                Quaternion rot = Quaternion.Euler(0f, _yaw, 0f);
                transform.position = target.position + rot * offset;
                transform.LookAt(target.position);
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        private void LateUpdate()
        {
            if (_target == null) return;
            if (Cursor.lockState == CursorLockMode.Locked)
                UpdateRotation();
            UpdatePosition();
        }

        private void UpdateRotation()
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            _yaw   += delta.x * sensitivity * Time.deltaTime;
            _pitch -= delta.y * sensitivity * Time.deltaTime;
            _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        private void UpdatePosition()
        {
            Quaternion rot    = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3    target = _target.position + rot * offset;
            transform.position = Vector3.Lerp(transform.position, target, followSmoothness * Time.deltaTime);
            transform.LookAt(_target.position + Vector3.up * 1.0f);
        }
    }
}