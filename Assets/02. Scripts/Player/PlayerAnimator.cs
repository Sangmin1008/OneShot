using Fusion;
using UnityEngine;

namespace OneShot
{
    public class PlayerAnimator : NetworkBehaviour
    {
        private static readonly int HASH_IS_MOVING = Animator.StringToHash("IsMoving");
        private static readonly int HASH_IS_SPRINTING = Animator.StringToHash("IsSprinting");
        private static readonly int HASH_MOVE_X = Animator.StringToHash("MoveX");
        private static readonly int HASH_MOVE_Y = Animator.StringToHash("MoveY");
        
        [Networked] private NetworkBool IsMoving { get; set; }
        [Networked] private NetworkBool IsSprinting { get; set; }
        [Networked] private Vector2 MoveBlend { get; set; }
        
        private Animator _animator;

        public override void Spawned()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        public override void FixedUpdateNetwork()
        {
            if (!GetInput(out PlayerNetworkInput input))
            {
                IsMoving = IsSprinting = false;
                MoveBlend = Vector2.zero;
                return;
            }
            
            bool moving = input.MoveDirection.sqrMagnitude > 0.01f;
            bool sprinting = input.Buttons.IsSet((int)PlayerButton.Sprint) && moving;

            IsMoving = moving;
            IsSprinting = sprinting;

            if (moving)
            {
                // 월드 좌표 -> 로컬 좌표 변환
                Vector3 worldMove = new Vector3(input.MoveDirection.x, 0, input.MoveDirection.y);
                Vector3 localMove = transform.InverseTransformDirection(worldMove);
                
                MoveBlend = new Vector2(localMove.x, localMove.z);
            }
            else
            {
                MoveBlend = Vector2.zero;
            }
        }

        // Update 사이클과 동일
        public override void Render()
        {
            _animator.SetFloat(HASH_MOVE_X, MoveBlend.x);
            _animator.SetFloat(HASH_MOVE_Y, MoveBlend.y);
        }
    }
}

