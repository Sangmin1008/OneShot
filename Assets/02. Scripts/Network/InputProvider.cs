using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OneShot
{
    public class InputProvider : MonoBehaviour, INetworkRunnerCallbacks
    {
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new PlayerNetworkInput();
            
            data.MoveDirection = GetMoveDirection();
            (data.AimDirection, data.AimTargetPoint) = GetAimInfo();
            
            // 버튼 입력 처리
            data.Buttons.Set((int)PlayerButton.Fire, Mouse.current.leftButton.isPressed);
            data.Buttons.Set((int)PlayerButton.Sprint, Keyboard.current.leftShiftKey.isPressed);
            data.Buttons.Set((int)PlayerButton.Reload, Keyboard.current.rKey.isPressed);

            input.Set(data);
        }

        #region 이동 입력값을 벡터로 변환

        private Vector2 GetMoveDirection()
        {
            if (Camera.main == null) return Vector2.zero;

            var kb = Keyboard.current;
            
            float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
            float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
            
            if (new Vector2(h, v).sqrMagnitude < 0.01f) return Vector2.zero;
            
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();
            
            Vector3 worldDir = ((camForward * v) + (camRight * h)).normalized;
            Logger.Log($"WASD: {worldDir}");
            
            return new Vector2(worldDir.x, worldDir.z);
        }

        #endregion

        #region 레이캐스트 조준 연산

        private (Vector3 direction, Vector3 targetPoint) GetAimInfo()
        {
            if (Camera.main == null) return (Vector3.forward, Vector3.forward * 200f);
            
            // 화면 중심으로 레이 발사
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, 200f))
            {
                Vector3 direction = (hit.point - Camera.main.transform.position).normalized;
                return (direction, hit.point);
            }
            
            // 아무것도 맞지 않은 경우 : 200m 전방 지점의 좌표 전달
            Vector3 farPoint = ray.origin + ray.direction * 200f;
            return (ray.direction, farPoint);
        }

        #endregion

        #region 미사용 퓨전 콜백

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {}
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {}
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {}
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
        public void OnConnectedToServer(NetworkRunner runner) {}
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {}
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}
        public void OnSceneLoadDone(NetworkRunner runner) {}
        public void OnSceneLoadStart(NetworkRunner runner) {}

        #endregion
    }
}