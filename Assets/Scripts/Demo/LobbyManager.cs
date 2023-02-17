using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;

namespace Demo
{
    [DefaultExecutionOrder(-999)]
    public class LobbyManager : MonoBehaviourSingletonPersistent<LobbyManager>
    {
        [SerializeField] float heartbeatTimerMax = 15f;
        [SerializeField] float lobbyPollTimerMax = 1.5f;
        [SerializeField] float refreshLobbyListTimerMax = 5f;

        public UnityEvent OnAuthenSignedIn;

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
                        Debug.Log($"{PlayerProfile.Name} has been kicked from lobby");
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

        private void HandleRefreshLobbyList()
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
            Debug.Log($"{PlayerProfile.Name} signed in");

            RefreshLobbyList();

            UILoading.Instance.Hide();

            OnAuthenSignedIn?.Invoke();
        }

        public async void Authen()
        {
            UILoading.Instance.Show();

            var initOption = new InitializationOptions()
                .SetOption(nameof(PlayerProfile.NAME_KEY), PlayerProfile.Name);

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

                Debug.Log($"OnLobbyListChanged {response.Results.Count} {response.ContinuationToken}");
                OnLobbyListChanged?.Invoke(response.Results, response.ContinuationToken);
            }
            catch(LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate, string gameMode)
        {
            try
            {
                CreateLobbyOptions options = new CreateLobbyOptions
                {
                    IsPrivate = isPrivate,
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            { PlayerProfile.NAME_KEY, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerProfile.Name) },
                            { PlayerProfile.AVATAR_KEY, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerProfile.Avatar.ToString()) }
                        }
                    },
                    Data = new Dictionary<string, DataObject>
                    {
                        { LobbyProfile.GAME_MODE_KEY, new DataObject(DataObject.VisibilityOptions.Public, gameMode, DataObject.IndexOptions.S1) }
                    }
                };

                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
                Debug.Log($"Lobby created [{lobby.Id}] : {lobby.Name} : {lobby.MaxPlayers} : {lobby.LobbyCode}");

                joinedLobby = lobby;
                OnJoinedLobby?.Invoke(joinedLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        public async Task<bool> KickPlayer(string playerId)
        {
            try
            {
                if (IsLobbyHost)
                {
                    await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);

                    Debug.Log($"Player [{playerId}] has been kicked");
                    return true;
                }
                else
                {
                    Debug.Log($"Only host player can kick");
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }

            return false;
        }

        public async void LeaveLobby()
        {
            try
            {
                if (IsJoinedLobby)
                {
                    var playerId = AuthenticationService.Instance.PlayerId;
                    await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
                    Debug.Log($"player {playerId} has left lobby {joinedLobby.Name}");

                    joinedLobby = null;
                    OnLeftLobby?.Invoke();
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}

public enum AVATAR_TYPE
{
    ONE,
    TWO
};

[Serializable]
public class PlayerProfile
{
    public string Name;
    public AVATAR_TYPE Avatar;

    public static string NAME_KEY => nameof(Name);
    public static string AVATAR_KEY => nameof(Avatar);
}

public enum GAME_MODE
{
    [StringValue("Global")]
    GLOBAL,

    [StringValue("Tennis")]
    TENNIS
}

[Serializable]
public class LobbyProfile
{
    public GAME_MODE Mode;

    public static string GAME_MODE_KEY => nameof(Mode);
}
