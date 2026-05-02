using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using OneShot;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = OneShot.Logger;

namespace OneShot
{
    
}
public class LobbyUI : MonoBehaviour, INetworkRunnerCallbacks
{
    private Dictionary<string, SessionInfo> _currentSessions = new();
    
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private Button serverButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_InputField sessionName;
    [SerializeField] private TextMeshProUGUI sessionListText;

    private void Start()
    {
        serverButton.interactable = false;
        hostButton.interactable = false;
        clientButton.interactable = false;
        
        networkManager = NetworkManager.Instance;
        // 러너 초기화 이벤트 수신
        networkManager.OnInitRunner += Initialize;
        serverButton.onClick.AddListener(OnServerClicked);
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
    }

    private void OnDestroy()
    {
        serverButton.onClick.RemoveAllListeners();
        hostButton.onClick.RemoveAllListeners();
        clientButton.onClick.RemoveAllListeners();
    }

    #region 핸들러 메서드

    private async void Initialize()
    {
        NetworkManager.Instance.Runner.AddCallbacks(this);
        await NetworkManager.Instance.Runner.JoinSessionLobby(SessionLobby.ClientServer);
        
        serverButton.interactable = true;
        hostButton.interactable = true;
        clientButton.interactable = true;
    }

    #endregion

    #region Fusion 콜백

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        // 호출 기준 : 신규 생성, 변경 / 하지만 삭제된건 호출되지 않아서 직접 삭제된걸 확인해야함
        // 새로 받은 세션 이름 목록저장
        var sessionNames = new HashSet<string>();
        foreach (var session in sessionList)
        {
            sessionNames.Add(session.Name);
        }
        
        // 세션 목록에 이전 세션명이 없으면 삭제
        foreach (var session in _currentSessions)
        {
            if (!sessionNames.Contains(session.Key))
            {
                Logger.Log($"세션 삭제: {session.Key}");
                // _currentSessions.Remove(session.Key);
            }
        }
        
        Logger.Log("=====================================");
        _currentSessions.Clear();
        sessionListText.text = "";
        
        // 출력 (ScrollView UI)
        foreach (var session in sessionList)
        {
            _currentSessions.Add(session.Name, session);
            Logger.Log($"Session Name: {session.Name}, Player: {session.PlayerCount}/{session.MaxPlayers}");
            sessionListText.text +=
                $"Session Name: {session.Name}, Player: {session.PlayerCount}/{session.MaxPlayers}\n";
        }
    }
    

    #endregion

    #region 버튼 콜백

    private void OnServerClicked()
    {
        networkManager.StartGame(GameMode.Server, sessionName.text);
    }

    private void OnHostClicked()
    {
        networkManager.StartGame(GameMode.Host, sessionName.text);
    }

    private void OnClientClicked()
    {
        networkManager.StartGame(GameMode.Client, sessionName.text);
    }

    #endregion

    #region 미사용 Fusion 콜백

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
    public void OnInput(NetworkRunner runner, NetworkInput input) {}
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
    public void OnConnectedToServer(NetworkRunner runner) {}
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}
    public void OnSceneLoadDone(NetworkRunner runner) {}
    public void OnSceneLoadStart(NetworkRunner runner) {}

    #endregion
   
}

