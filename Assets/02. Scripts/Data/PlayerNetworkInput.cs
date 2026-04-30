using Fusion;
using UnityEngine;

namespace OneShot
{
    // 매 네트워크 Tick 마다 전송되는 플레이어 입력 데이터를 정의하는 구조체
    public struct PlayerNetworkInput : INetworkInput
    {
        public Vector2 MoveDirection;
        public Vector3 AimDirection;
        public Vector3 AimTargetPoint;
        public NetworkButtons Buttons;
    }

    public enum PlayerButton
    {
        Fire = 0,
        Sprint = 1,
        Reload = 2,
    }
}