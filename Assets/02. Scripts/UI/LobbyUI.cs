using System;
using Fusion;
using OneShot;
using UnityEngine;
using UnityEngine.UI;

namespace OneShot
{
    
}
public class LobbyUI : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private Button serverButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private void Start()
    {
        networkManager = NetworkManager.Instance;
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

    private void OnServerClicked()
    {
        networkManager.StartGame(GameMode.Server);
    }

    private void OnHostClicked()
    {
        networkManager.StartGame(GameMode.Host);
    }

    private void OnClientClicked()
    {
        networkManager.StartGame(GameMode.Client);
    }
}

