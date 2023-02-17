using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class UICreateLobby : UIBase<UICreateLobby>
    {
        [SerializeField] Button btnCreate;

        private void OnEnable()
        {
            btnCreate.onClick.AddListener(OnCreateLobby);
            LobbyManager.Instance.OnJoinedLobby.AddListener(OnJoinedLobby);
        }

        private void OnDisable()
        {
            btnCreate.onClick.RemoveListener(OnCreateLobby);
            LobbyManager.Instance.OnJoinedLobby.RemoveListener(OnJoinedLobby);
        }

        private void OnJoinedLobby(Lobby lobby)
        {
            this.Hide();
        }

        private void OnCreateLobby()
        {
            LobbyManager.Instance.CreateLobby("test", UnityEngine.Random.Range(2, 10), false, GAME_MODE.TENNIS);
        }
    }
}
