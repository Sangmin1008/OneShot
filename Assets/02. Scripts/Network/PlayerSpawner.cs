using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using OneShot;
using UnityEngine;
using Logger = OneShot.Logger;
using Random = UnityEngine.Random;

public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private float respawnTime = 3f;
    
    // PlayerRef : 플레이어 정보를 식별하는 구조체
    private readonly Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new();

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // Spawn은 서버(Host)에서만 실행
        if (!runner.IsServer) return;

        SpawnPlayer(player, runner);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        DespawnPlayer(player, runner);
        Logger.Log($"[PlayerSpawner] 플레이어 퇴장: {player}");
    }



    // 플레이어 생성 메서드
    private void SpawnPlayer(PlayerRef player, NetworkRunner runner)
    {
        // 랜덤 좌표
        Vector3 spawnPos = new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
        
        // 플레이어 오브젝트 생성
        NetworkObject playerObject = runner.Spawn(
            prefabRef: playerPrefab,
            position: spawnPos,
            rotation: Quaternion.identity,
            inputAuthority: player
        );
        
        // PlayerRef를 등록
        runner.SetPlayerObject(player, playerObject);
        // 딕셔너리 추가
        _spawnedPlayers[player] = playerObject;
        
        // 사망 이벤트 연결(구독)
        if (playerObject.TryGetComponent<PlayerHealth>(out var health))
        {
            health.OnDeath += () => HandlePlayerDeath(player, runner);
        }

        Logger.Log($"[PlayerSpawn] 플레이어 스폰: {player}");
    }
    
    private void DespawnPlayer(PlayerRef player, NetworkRunner runner)
    {
        if (_spawnedPlayers.TryGetValue(player, out NetworkObject playerObject))
        {
            runner.Despawn(playerObject);
            _spawnedPlayers.Remove(player);
        }
    }

    private void HandlePlayerDeath(PlayerRef player, NetworkRunner runner)
    {
        DespawnPlayer(player, runner);
        StartCoroutine(RespawnPlayer(player, runner));
    }
    
    // 리스폰 코루틴
    private IEnumerator RespawnPlayer(PlayerRef player, NetworkRunner runner)
    {
        yield return new WaitForSeconds(respawnTime);
        
        if (runner == null || !runner.IsRunning) yield break;
        
        SpawnPlayer(player, runner);
        Logger.Log($"[PlayerSpawner] 플레이어 리스폰: {player}");
    }


    #region INetworkRunnerCallbacks

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {}
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}
    public void OnInput(NetworkRunner runner, NetworkInput input) {}
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
    public void OnConnectedToServer(NetworkRunner runner) {}
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {}
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}
    public void OnSceneLoadDone(NetworkRunner runner) {}
    public void OnSceneLoadStart(NetworkRunner runner) {}
    
    #endregion
}
