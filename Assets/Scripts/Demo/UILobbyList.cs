using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class UILobbyList : UIBase<UILobbyList>
    {
        [SerializeField] GameObject listItemPrefab;
        [SerializeField] Transform listContainer;
        [SerializeField] List<Color> listItemColor;

        [SerializeField] Button btnCreate;
        [SerializeField] Button btnRefresh;


        private void OnEnable()
        {
            btnCreate.onClick.AddListener(OnCreateLobby);
            btnRefresh.onClick.AddListener(OnRefreshLobby);

            LobbyManager.Instance.OnAuthenSignedIn.AddListener(OnAuthenSignedIn);
            LobbyManager.Instance.OnLobbyListChanged.AddListener(OnLobbyListChanged);
            LobbyManager.Instance.OnJoinedLobby.AddListener(OnJoinedLobby);
            LobbyManager.Instance.OnLeftLobby.AddListener(OnLeftLobby);
            LobbyManager.Instance.OnKickedFromLobby.AddListener(OnKickedFromLobby);
        }

        private void OnDisable()
        {
            btnCreate.onClick.RemoveListener(OnCreateLobby);
            btnRefresh.onClick.RemoveListener(OnRefreshLobby);

            LobbyManager.Instance.OnAuthenSignedIn.RemoveListener(OnAuthenSignedIn);
            LobbyManager.Instance.OnLobbyListChanged.RemoveListener(OnLobbyListChanged);
            LobbyManager.Instance.OnJoinedLobby.RemoveListener(OnJoinedLobby);
            LobbyManager.Instance.OnLeftLobby.RemoveListener(OnLeftLobby);
            LobbyManager.Instance.OnKickedFromLobby.RemoveListener(OnKickedFromLobby);
        }

        private void OnRefreshLobby()
        {
            LobbyManager.Instance.RefreshLobbyList();
        }

        private void OnCreateLobby()
        {
            UICreateLobby.Instance.Show();
        }

        private void OnAuthenSignedIn()
        {
            this.Show();
        }

        private void OnKickedFromLobby(Lobby lobby)
        {
            this.Show();
        }

        private void OnLeftLobby()
        {
            this.Show();
        }

        private void OnJoinedLobby(Lobby lobby)
        {
            this.Hide();
        }

        private void OnLobbyListChanged(List<Lobby> lobbies, string continueToken)
        {
            ClearLobbiesList();
            UpdateLobbyList(lobbies);
        }

        private void ClearLobbiesList()
        {
            foreach(Transform child in listContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void UpdateLobbyList(List<Lobby> lobbies)
        {
            for(int i=0; i < lobbies.Count; i++)
            {
                var lobby = lobbies[i];
                var item = Instantiate(listItemPrefab, listContainer);
                var lobbyItem = item.GetComponent<LobbyItem>();
                if (item != null)
                {
                    lobbyItem.LobbyId = lobby.Id;
                    lobbyItem.SetLobbyName(lobby.Name);
                    lobbyItem.SetLobbyPlayers($"{lobby.Players.Count} / {lobby.MaxPlayers}");
                    lobbyItem.SetBackgroundColor(listItemColor[i % listItemColor.Count]);

                    if (lobby.Data != null && lobby.Data.ContainsKey(LobbyProfile.GAME_MODE_KEY))
                    {
                        lobbyItem.SetLobbyGameMode(lobby.Data[LobbyProfile.GAME_MODE_KEY].Value);
                    }
                    else
                    {
                        lobbyItem.SetLobbyGameMode("");
                    }
                }
            }
        }
    }
}
