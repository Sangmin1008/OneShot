using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace OneShot
{
    public class NetworkManager : Singleton<NetworkManager>
    {
        [SerializeField] private int gameSceneIndex = 1;
        [SerializeField] private string sessionName = "MyRoom";
        
        private NetworkRunner _runner;
        public NetworkRunner Runner => _runner;
        public event Action OnInitRunner;

        public override void Awake()
        {
            base.Awake();
            
            // NetworkRunner 생성
            _runner = gameObject.AddComponent<NetworkRunner>();

        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(2.0f);
            OnInitRunner?.Invoke();
        }
        

        public async void StartGame(GameMode mode, string currentSessionName)
        {
            sessionName = currentSessionName;
            
            // 입력 권한 설정
            _runner.ProvideInput = (mode != GameMode.Server);
            
            var result = await _runner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                SessionName = sessionName,
                Scene = SceneRef.FromIndex(gameSceneIndex),
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            });

            if (!result.Ok)
            {
                Logger.Log($"게임 시작 실패: {result.ShutdownReason}");
            }
            else
            {
                Logger.Log($"게임 시작 성공: {mode}");
            }
        }
    }
}