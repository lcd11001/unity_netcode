using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;

namespace Demo
{
    public class LobbyManager : MonoBehaviourSingletonPersistent<LobbyManager>
    {
        [SerializeField] float heartbeatTimerMax = 15f;
        [SerializeField] float lobbyPollTimerMax = 1.5f;
        [SerializeField] float refreshLobbyListTimerMax = 5f;

        public UnityEvent<Lobby> OnJoinedLobby;
        public UnityEvent<Lobby> OnJoinedLobbyUpdate;
        public UnityEvent<Lobby> OnKickedFromLobby;
        public UnityEvent<Lobby> OnLobbyGameModeChanged;
        
        public UnityEvent OnLeftLobby;

        public UnityEvent<List<Lobby>, string> OnLobbyListChanged;


        private Lobby joinedLobby;
        private float heartbeatTimer;
        private float lobbyPollTimer;
        private float refreshLobbyListTimer;

        public PlayerProfile PlayerProfile { get; private set; } = new PlayerProfile();

        public bool IsLobbyHost => joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
        public bool IsJoinedLobby => joinedLobby != null;

        public bool IsPlayerInLooby
        {
            get
            {
                if (joinedLobby != null && joinedLobby.Players != null)
                {
                    foreach(var player in joinedLobby.Players)
                    {
                        if (player.Id == AuthenticationService.Instance.PlayerId)
                        {
                            // this player in lobby
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        private void Update()
        {
            //HandleRefreshLobbyList();
            HandleLobbyHeartBeat();
            HandleLobbyPolling();
        }

        private async void HandleLobbyPolling()
        {
            if (IsJoinedLobby)
            {
                lobbyPollTimer -= Time.deltaTime;
                if (lobbyPollTimer < 0)
                {
                    lobbyPollTimer = lobbyPollTimerMax;
                    joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

                    OnJoinedLobbyUpdate?.Invoke(joinedLobby);

                    if (!IsPlayerInLooby)
                    {
                        Debug.Log($"{PlayerProfile.PlayerName} has been kicked from lobby");
                        OnKickedFromLobby?.Invoke(joinedLobby);
                        joinedLobby = null;
                    }
                }
            }
        }

        private async void HandleLobbyHeartBeat()
        {
            if (IsLobbyHost)
            {
                heartbeatTimer -= Time.deltaTime;
                if (heartbeatTimer < 0)
                {
                    heartbeatTimer = heartbeatTimerMax;
                    await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
                }
            }
        }

        private async void HandleRefreshLobbyList()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
            {
                refreshLobbyListTimer -= Time.deltaTime;
                if (refreshLobbyListTimer < 0)
                {
                    refreshLobbyListTimer = refreshLobbyListTimerMax;

                    RefreshLobbyList();
                }
            }
        }

        private void OnSignedIn()
        {
            Debug.Log($"{PlayerProfile.PlayerName} signed in");

            RefreshLobbyList();

            UILoading.Instance.Hide();
        }

        public async void Authen()
        {
            UILoading.Instance.Show();

            var initOption = new InitializationOptions()
                .SetOption(nameof(PlayerProfile.PlayerName), PlayerProfile.PlayerName);

            await UnityServices.InitializeAsync(initOption);

            AuthenticationService.Instance.SignedIn += OnSignedIn;

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        public async void RefreshLobbyList(string continueToken = null)
        {
            try
            {
                QueryLobbiesOptions options = new QueryLobbiesOptions
                {
                    Count = 25,

                    ContinuationToken = continueToken,

                    // filter for open lobbies only
                    Filters = new List<QueryFilter>
                    {
                        new QueryFilter(
                            field: QueryFilter.FieldOptions.AvailableSlots,
                            op: QueryFilter.OpOptions.GT,
                            value: "0"
                        )
                    },

                    // order by newest lobbies first
                    Order = new List<QueryOrder>
                    {
                        new QueryOrder(
                            asc: false,
                            field: QueryOrder.FieldOptions.Created
                        )
                    }
                };

                var response = await LobbyService.Instance.QueryLobbiesAsync(options);

                OnLobbyListChanged?.Invoke(response.Results, response.ContinuationToken);
            }
            catch(LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}

[Serializable]
public class PlayerProfile
{
    public string PlayerName;
}
