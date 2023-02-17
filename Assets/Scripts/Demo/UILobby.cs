using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Demo
{
    public class UILobby : UIBase<UILobby>
    {
        private void OnEnable()
        {
            LobbyManager.Instance.OnJoinedLobby.AddListener(OnJoinedLobby);
        }

        private void OnDisable()
        {
            LobbyManager.Instance.OnJoinedLobby.RemoveListener(OnJoinedLobby);
        }

        private void OnJoinedLobby(Lobby lobby)
        {
            this.Show();
        }
    }
}
