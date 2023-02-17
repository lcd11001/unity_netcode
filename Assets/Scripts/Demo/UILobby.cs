using System;
using System.Collections;
using System.Collections.Generic;
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
            btnBack.onClick.AddListener(OnExitLobby);
            btnStart.onClick.AddListener(OnStartGame);
            LobbyManager.Instance.OnJoinedLobby.AddListener(OnJoinedLobby);
            LobbyManager.Instance.OnJoinedLobbyUpdate.AddListener(OnJoinedLobbyUpdate);
            LobbyManager.Instance.OnLeftLobby.AddListener(OnLeftLobby);
        }

        private void OnDisable()
        {
            btnBack.onClick.RemoveListener(OnExitLobby);
            btnStart.onClick.RemoveListener(OnStartGame);
            LobbyManager.Instance.OnJoinedLobby.RemoveListener(OnJoinedLobby);
            LobbyManager.Instance.OnJoinedLobbyUpdate.RemoveListener(OnJoinedLobbyUpdate);
            LobbyManager.Instance.OnLeftLobby.RemoveListener(OnLeftLobby);
        }

        private void OnStartGame()
        {
            Debug.Log("OnStartGame");
        }

        private void OnExitLobby()
        {
            LobbyManager.Instance.LeaveLobby();
        }

        private void OnLeftLobby()
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
            this.Show();
            RefreshPlayerList(lobby);
        }

        private void RefreshPlayerList(Lobby lobby)
        {
            if (lobby.Players != null)
            {
                foreach (var player in lobby.Players)
                {
                    var item = Instantiate(playerItemPrefab, listContainer);
                    var playerItem = item.GetComponent<LobbyPlayerItem>();

                    playerItem.PlayerId = player.Id;
                    playerItem.EnableKickButton (player.Id != lobby.HostId);
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
