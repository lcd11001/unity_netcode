using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
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
        }

        private void OnJoinedLobby(Lobby lobby)
        {
            ClearPlayerList();
            this.Show();
            RefreshPlayerList(lobby);
        }

        private void OnGameStarted(bool isHost)
        {
            this.Hide();
            if (isHost)
            {
                NetworkManager.Singleton.StartHost();
            }
            else
            {
                NetworkManager.Singleton.StartClient();
            }
        }

        private void RefreshPlayerList(Lobby lobby)
        {
            if (lobby.Players != null)
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
