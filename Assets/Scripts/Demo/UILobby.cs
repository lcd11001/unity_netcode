using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Demo
{
    public class UILobby : UIBase<UILobby>
    {
        [SerializeField] GameObject playerItemPrefab;
        [SerializeField] Transform listContainer;
        [SerializeField] Button btnBack;
        [SerializeField] Button btnStart;

        private void OnEnable()
        {
            btnBack.onClick.AddListener(OnExitLobbyClicked);
            btnStart.onClick.AddListener(OnStartGameClicked);

            LobbyManager.Instance.OnJoinedLobby.AddListener(OnJoinedLobby);
            LobbyManager.Instance.OnJoinedLobbyUpdate.AddListener(OnJoinedLobbyUpdate);
            LobbyManager.Instance.OnLeftLobby.AddListener(OnLeftLobby);
            LobbyManager.Instance.OnKickedFromLobby.AddListener(OnKickedFromLobby);

            LobbyManager.Instance.OnGameStarted.AddListener(OnGameStarted);
        }

        private void OnDisable()
        {
            btnBack.onClick.RemoveListener(OnExitLobbyClicked);
            btnStart.onClick.RemoveListener(OnStartGameClicked);

            LobbyManager.Instance.OnJoinedLobby.RemoveListener(OnJoinedLobby);
            LobbyManager.Instance.OnJoinedLobbyUpdate.RemoveListener(OnJoinedLobbyUpdate);
            LobbyManager.Instance.OnLeftLobby.RemoveListener(OnLeftLobby);
            LobbyManager.Instance.OnKickedFromLobby.RemoveListener(OnKickedFromLobby);

            LobbyManager.Instance.OnGameStarted.RemoveListener(OnGameStarted);
        }

        private void OnStartGameClicked()
        {
            UILoading.Instance.Show();
            LobbyManager.Instance.StartGame();
        }

        private void OnExitLobbyClicked()
        {
            LobbyManager.Instance.LeaveLobby();
        }

        private void OnLeftLobby()
        {
            Hide();
        }

        private void OnKickedFromLobby(Lobby lobby)
        {
            Hide();
        }

        private void OnJoinedLobbyUpdate(Lobby lobby)
        {
            ClearPlayerList();
            RefreshPlayerList(lobby);
            UpdateHostLobby();
        }

        private void UpdateHostLobby()
        {
            btnStart.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost);
        }

        private void OnJoinedLobby(Lobby lobby)
        {
            ClearPlayerList();
            this.Show();
            RefreshPlayerList(lobby);
        }

        private void OnGameStarted(bool isHost)
        {
            UILoading.Instance.Show();

            StartCoroutine(LoadGameSceneAsync(isHost));
        }

        IEnumerator LoadGameSceneAsync(bool isHost)
        {
            if (isHost)
            {
                NetworkManager.Singleton.StartHost();
            }
            else
            {
                NetworkManager.Singleton.StartClient();
            }

            LoadingSceneManager.Instance.LoadScene(SceneName.DEMO_GAME, true);
            while (LoadingSceneManager.Instance.IsLoading)
            {
                //Debug.Log("IsLoading");
                yield return null;
            }

            //AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("DemoGame", LoadSceneMode.Single);
            //asyncLoad.allowSceneActivation = true;
            //while (!asyncLoad.isDone)
            //{
            //    yield return null;
            //}

            UILoading.Instance.Hide();
            this.Hide();
        }

        private void RefreshPlayerList(Lobby lobby)
        {
            if (lobby != null && lobby.Players != null)
            {
                foreach (var player in lobby.Players)
                {
                    var item = Instantiate(playerItemPrefab, listContainer);
                    var playerItem = item.GetComponent<LobbyPlayerItem>();
                    var isMe = player.Id == AuthenticationService.Instance.PlayerId;
                    playerItem.PlayerId = player.Id;
                    playerItem.EnableKickButton (LobbyManager.Instance.IsLobbyHost && !isMe);
                    if (player.Data != null)
                    {
                        if (player.Data.ContainsKey(PlayerProfile.NAME_KEY))
                        {
                            playerItem.SetPlayerName(player.Data[PlayerProfile.NAME_KEY].Value);
                        }

                        if (player.Data.ContainsKey(PlayerProfile.AVATAR_KEY))
                        {
                            //playerItem.SetPlayerIcon();
                        }
                    }
                }
            }
        }

        private void ClearPlayerList()
        {
            foreach(Transform child in listContainer)
            {
                Destroy(child.gameObject);
            }
        }

        
    }
}
