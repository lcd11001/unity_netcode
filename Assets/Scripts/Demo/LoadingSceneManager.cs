using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Demo
{

    public class LoadingSceneManager : MonoBehaviourSingletonPersistent<LoadingSceneManager>
    {
        [field: SerializeField] public SceneName SceneActive;

        public bool IsLoading { get; private set; } = false;

        private IEnumerator InitNetworkLoading()
        {
            yield return new WaitUntil(() => NetworkManager.Singleton.SceneManager != null);
            // Set the events on the loading manager
            // Doing this because every time the network session ends the loading manager stops detecting the events
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnNetworkLoadSceneComplete;

            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnNetworkLoadSceneComplete;
        }

        //private IEnumerator Start()
        //{
        //    Debug.Log("LoadingSceneManager::start");
        //    yield return new WaitUntil(() => NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null);
        //}

        //private void Destroy()
        //{
        //    Debug.Log("LoadingSceneManager::Destroy");
        //}

        //private void OnEnable()
        //{
        //    Debug.Log("LoadingSceneManager::OnEnable");
        //}

        //private void OnDisable()
        //{
        //    Debug.Log("LoadingSceneManager::OnDisable");
        //}

        public void LoadScene(SceneName sceneToLoad, bool isNetworkSessionActive)
        {
            IsLoading = true;
            StartCoroutine(Loading(sceneToLoad, isNetworkSessionActive));
        }

        private IEnumerator Loading(SceneName sceneToLoad, bool isNetworkSessionActive)
        {
            // [TODO]: Fading effect
            yield return null;

            if (isNetworkSessionActive)
            {
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
                {
                    yield return InitNetworkLoading();
                    LoadSceneNetwork(sceneToLoad);
                }
            }
            else
            {
                yield return LoadSceneLocal(sceneToLoad);
            }
        }

        private IEnumerator LoadSceneLocal(SceneName sceneToLoad)
        {
            var operation = SceneManager.LoadSceneAsync(sceneToLoad.GetStringValue());
            while(!operation.isDone)
            {
                yield return null;
            }

            SceneActive = sceneToLoad;
            IsLoading = false;
        }

        private void LoadSceneNetwork(SceneName sceneToLoad)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad.GetStringValue(), LoadSceneMode.Single);
        }

        private void OnNetworkLoadSceneComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            IsLoading = false;

            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            if (sceneName == SceneName.DEMO_GAME.GetStringValue())
            {
                SceneActive = SceneName.DEMO_GAME;
                GameManager.Instance.ServerSceneInit(clientId);
            }
        }
    }

    public enum SceneName
    {
        NONE,
        [StringValue("DemoLobby")]
        DEMO_LOBBY,
        [StringValue("DemoGame")]
        DEMO_GAME
    }
}
