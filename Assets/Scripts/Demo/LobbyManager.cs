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

        public UnityEvent<bool> OnGameStarted;

        private Lobby joinedLobby;
        private float heartbeatTimer;
        private float lobbyPollTimer;
        private float refreshLobbyListTimer;

        public bool IsGameHost { get; private set; } = true;

        public PlayerProfile PlayerProfile { get; private set; } = new PlayerProfile();

        public bool IsLobbyHost => joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
        public bool IsJoinedLobby => joinedLobby != null;

        public string RelayCode { get; private set; } = "";

        public bool IsPlayerInLooby
        {
            get
            {
                if (joinedLobby != null && joinedLobby.Players != null)
                {
                    foreach (var player in joinedLobby.Players)
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

        public bool IsReceivedRelayCode
        {
            get
            {
                if (IsJoinedLobby)
                {
                    if (joinedLobby.Data != null && joinedLobby.Data.ContainsKey(LobbyProfile.RELAY_JOIN_CODE_KEY))
                    {
                        RelayCode = joinedLobby.Data[LobbyProfile.RELAY_JOIN_CODE_KEY].Value;
                        if (!string.IsNullOrEmpty(RelayCode))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        private Dictionary<string, PlayerDataObject> GetPlayerData()
        {
            return new Dictionary<string, PlayerDataObject>
            {
                { PlayerProfile.NAME_KEY, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerProfile.Name) },
                { PlayerProfile.AVATAR_KEY, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerProfile.Avatar.ToString()) }
            };
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

                    if (IsReceivedRelayCode)
                    {
                        IsGameHost = IsLobbyHost;
                        // start game
                        if (!IsGameHost)
                        {
                            // lobby host already joined relay
                            await RelayManager.Instance.JoinRelay(RelayCode);
                        }

                        OnGameStarted?.Invoke(IsGameHost);
                        joinedLobby = null;
                    }

                    else if (!IsPlayerInLooby)
                    {
                        Debug.Log($"{PlayerProfile.Name} has been kicked from lobby");
                        OnKickedFromLobby?.Invoke(joinedLobby);
                        joinedLobby = null;
                    }

                    OnJoinedLobbyUpdate?.Invoke(joinedLobby);
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
            Debug.Log($"{PlayerProfile.Name} signed in with id {AuthenticationService.Instance.PlayerId}");

            RefreshLobbyList();

            UILoading.Instance.Hide();

            OnAuthenSignedIn?.Invoke();
        }

        public async void Authen()
        {
            UILoading.Instance.Show();

            var initOption = new InitializationOptions()
                // must use SetProfile to generate difference PlayerId after signed in
                .SetProfile(PlayerProfile.Name);

            await UnityServices.InitializeAsync(initOption);

            AuthenticationService.Instance.SignedIn += OnSignedIn;

            var signInOption = new SignInOptions
            {
                CreateAccount = true
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync(signInOption);
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

                Debug.Log($"OnLobbyListChanged {response.Results.Count}");
                OnLobbyListChanged?.Invoke(response.Results, response.ContinuationToken);
            }
            catch (LobbyServiceException e)
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
                        Data = GetPlayerData()
                    },
                    Data = new Dictionary<string, DataObject>
                    {
                        { LobbyProfile.GAME_MODE_KEY, new DataObject(DataObject.VisibilityOptions.Public, gameMode, DataObject.IndexOptions.S1) },
                        { LobbyProfile.RELAY_JOIN_CODE_KEY, new DataObject(DataObject.VisibilityOptions.Member, "", DataObject.IndexOptions.S2) }
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

        public async void JoinLobbyById(string id)
        {
            try
            {
                JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
                {
                    Player = new Player
                    {
                        Data = GetPlayerData()
                    }
                };
                var lobby = await LobbyService.Instance.JoinLobbyByIdAsync(id, options);
                Debug.Log($"Joined to lobby {lobby.Name} : {lobby.AvailableSlots}");

                joinedLobby = lobby;
                OnJoinedLobby?.Invoke(joinedLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        public async void JoinLobbyByCode(string code)
        {
            try
            {
                JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
                {
                    Player = new Player
                    {
                        Data = GetPlayerData()
                    }
                };
                var lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code, options);
                Debug.Log($"Joined to lobby {lobby.Name} : {lobby.AvailableSlots}");

                joinedLobby = lobby;
                OnJoinedLobby?.Invoke(joinedLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        public async void StartGame()
        {
            try
            {
                if (IsLobbyHost)
                {
                    string relayCode = await RelayManager.Instance.CreateRelay(joinedLobby.MaxPlayers);

                    Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                    {
                        Data = new Dictionary<string, DataObject>
                        {
                            {  LobbyProfile.RELAY_JOIN_CODE_KEY, new DataObject(DataObject.VisibilityOptions.Member, relayCode, DataObject.IndexOptions.S2) }
                        }
                    });

                    joinedLobby = lobby;
                }
                else
                {
                    Debug.Log($"Only host player can start game");
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
    public string RelayJoinCode;
    public static string GAME_MODE_KEY => nameof(Mode);
    public static string RELAY_JOIN_CODE_KEY => nameof(RelayJoinCode);
}
