using System;
using Fusion;
using UnityEngine;

namespace OneShot
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        [SerializeField] private Transform cameraPivot;

        // AOI 반경 - Sphere만 가능
        [SerializeField] private float aoiRadius = 20f;
        
        private NetworkCharacterController _cc;

        public override void Spawned()
        {
            _cc = GetComponent<NetworkCharacterController>();
            
            // 자신의 로컬 플레이어 여부 HasInputAuthority
            if (HasInputAuthority)
            {
                var cam = FindFirstObjectByType<TPSCameraController>();
                cam.SetTarget(cameraPivot);
            }

            if (Runner.IsServer)
            {
                Runner.AddPlayerAreaOfInterest(Object.InputAuthority, transform.position, aoiRadius);
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (Runner.IsServer)
            {
                Runner.AddPlayerAreaOfInterest(Object.InputAuthority, transform.position, aoiRadius);
            }
            
            // GetInput : 서버와 클라이언트가 동일한 입력값을 제공
            if (!GetInput(out PlayerNetworkInput input)) return;
            
            // 이동 처리
            Move(input.MoveDirection, input.Buttons.IsSet((int)PlayerButton.Sprint));
            // 회전 처리
            FaceAimDirection(input.AimDirection);
        }

        private void Move(Vector2 moveDir, bool isSprinting)
        {
            if (moveDir.sqrMagnitude < 0.01f)
            {
                // 입력을 무시할 때 관성을 제거
                _cc.Velocity = new Vector3(0f, _cc.Velocity.y, 0f);
                return;
            }
            
            Vector3 moveDirection = new Vector3(moveDir.x, 0f, moveDir.y);
            float speed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
            
            // NCC 내부의 MaxSpeed 동적으로 변경
            // Time.DeltaTime, Time.FixedDeltaTime => Runner.DeltaTime
            _cc.maxSpeed = speed;
            _cc.Move(moveDirection * moveSpeed * Runner.DeltaTime);
        }

        private void FaceAimDirection(Vector3 aimDir)
        {
            Vector3 rotDir = new Vector3(aimDir.x, 0f, aimDir.z);
            if (rotDir.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.LookRotation(rotDir);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, aoiRadius);
        }
    }
}

