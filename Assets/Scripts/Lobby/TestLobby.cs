using SmartConsole;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TestLobby : CommandBehaviour
{
    private Lobby hostLobby;
    private Lobby joinLobby;

    [SerializeField]
    private float heartBeatTimerMax = 15f;

    [SerializeField]
    private float lobbyUpdateTimerMax = 1.5f;

    [SerializeField]
    private string playerName;

    private float heartBeatTimer;
    private float lobbyUpdateTime;
    private string playerId;

    protected async override void Start()
    {
        base.Start();

        heartBeatTimer = heartBeatTimerMax;
        lobbyUpdateTime = lobbyUpdateTimerMax;

        if (string.IsNullOrEmpty(playerName))
        {
            playerName = $"LCD{Random.Range(10, 100)}";
        }

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            playerId = AuthenticationService.Instance.PlayerId;
            Debug.Log($"Signed in {playerId}");
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPollForUpdate();
    }

    private async void HandleLobbyHeartBeat()
    {
        if (hostLobby != null)
        {
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer < 0f)
            {
                heartBeatTimer = heartBeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdate()
    {
        try
        {
            if (joinLobby != null)
            {
                lobbyUpdateTime -= Time.deltaTime;
                if (lobbyUpdateTime < 0f)
                {
                    lobbyUpdateTime = lobbyUpdateTimerMax;

                    var lobby = await LobbyService.Instance.GetLobbyAsync(joinLobby.Id);
                    joinLobby = lobby;

                    if (hostLobby == null && lobby.HostId == playerId)
                    {
                        hostLobby = lobby;
                        Debug.Log($"{playerId} is host now");
                    }
                }
            }
        }
        catch(LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                Debug.Log($"lobby {joinLobby.Name} : {joinLobby.LobbyCode} has been deleted");
                joinLobby = null;
            }
        }
    }

    [Command]
    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { TestPlayerData.PLAYER_NAME.GetStringValue(), new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                    }
                },
                Data = new Dictionary<string, DataObject>
                {
                    { TestPlayerData.GAME_MODE.GetStringValue(), new DataObject(DataObject.VisibilityOptions.Public, "Tennis", DataObject.IndexOptions.S1) },
                    { TestPlayerData.MAP.GetStringValue(), new DataObject(DataObject.VisibilityOptions.Public, "de_dust2", DataObject.IndexOptions.S2) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            Debug.Log($"Lobby created [{lobby.Id}] : {lobby.Name} : {lobby.MaxPlayers} : {lobby.LobbyCode}");

            hostLobby = lobby;
            joinLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void ListLobbiesByOptions(QueryLobbiesOptions options)
    {
        try
        {
            var response = await Lobbies.Instance.QueryLobbiesAsync(options);

            Debug.Log($"Lobbies found {response.Results.Count}");
            Debug.Log("ID : Name : AvailableSlots : LobbyCode : GAME_MODE : MAP");
            foreach (var lobby in response.Results)
            {
                Debug.Log($"{lobby.Id} : {lobby.Name} : {lobby.AvailableSlots} : {lobby.LobbyCode} : {lobby.Data[TestPlayerData.GAME_MODE.GetStringValue()].Value} : {lobby.Data[TestPlayerData.MAP.GetStringValue()].Value}");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private void ListLobbies()
    {
        try
        {
            ListLobbiesByOptions(null);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private void ListLobbiesByName(string filterName)
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    new QueryFilter(QueryFilter.FieldOptions.Name, filterName, QueryFilter.OpOptions.CONTAINS)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(true, QueryOrder.FieldOptions.AvailableSlots),
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            ListLobbiesByOptions(options);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private void ListLobbiesByGameMode(string gameMode)
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    new QueryFilter(QueryFilter.FieldOptions.S1, gameMode, QueryFilter.OpOptions.EQ)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(true, QueryOrder.FieldOptions.AvailableSlots),
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            ListLobbiesByOptions(options);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void JoinLobbyById(string id)
    {
        try
        {
            var lobby = await Lobbies.Instance.JoinLobbyByIdAsync(id);
            Debug.Log($"Joined to lobby {lobby.Name} : {lobby.AvailableSlots}");

            joinLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void JoinLobbyByCode(string code)
    {
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { TestPlayerData.PLAYER_NAME.GetStringValue(), new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                    }
                }
            };
            var lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code, options);
            Debug.Log($"Joined to lobby {lobby.Name} : {lobby.AvailableSlots}");

            joinLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { TestPlayerData.PLAYER_NAME.GetStringValue(), new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                    }
                }
            };

            var lobby = await Lobbies.Instance.QuickJoinLobbyAsync(options);
            Debug.Log($"Joined to lobby {lobby.Name} : {lobby.AvailableSlots}");

            joinLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void LeaveLobby()
    {
        try
        {
            if (joinLobby != null && !string.IsNullOrEmpty(playerId))
            {
                await LobbyService.Instance.RemovePlayerAsync(joinLobby.Id, playerId);
                Debug.Log($"player {playerId} has leave lobby {joinLobby.Name}");

                if (hostLobby != null && joinLobby.Id == hostLobby.Id)
                {
                    Debug.Log($"close host lobby");
                    hostLobby = null;
                }
                joinLobby = null;
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void ListPlayers(Lobby lobby)
    {
        if (lobby != null)
        {
            Debug.Log($"Players in lobby {lobby.Name} game mode {lobby.Data[TestPlayerData.GAME_MODE.GetStringValue()].Value} map {lobby.Data[TestPlayerData.MAP.GetStringValue()].Value}");

            // only member of lobby can get the list of players Data
            foreach (var player in lobby.Players)
            {
                Debug.Log($"{(player.Id == lobby.HostId ? "*" : " ")} [{player.Id}] {(player.Data != null ? player.Data[TestPlayerData.PLAYER_NAME.GetStringValue()]?.Value : "")}");
            }
        }
    }

    [Command]
    private void ListPlayers()
    {
        ListPlayers(joinLobby);
    }

    [Command]
    private async void ChangeLobbyGameMode(string newGameMode)
    {
        try
        {
            if (joinLobby != null && joinLobby.HostId == playerId)
            {
                var lobby = await Lobbies.Instance.UpdateLobbyAsync(joinLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { TestPlayerData.GAME_MODE.GetStringValue(), new DataObject(DataObject.VisibilityOptions.Public, newGameMode, DataObject.IndexOptions.S1) }
                    }
                });

                Debug.Log($"lobby {lobby.Name} has changed game mode from {joinLobby.Data[TestPlayerData.GAME_MODE.GetStringValue()].Value} to {lobby.Data[TestPlayerData.GAME_MODE.GetStringValue()].Value} ");

                joinLobby = lobby;
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void ChangePlayerName(string newName)
    {
        try
        {
            if (joinLobby != null)
            {
                var lobby = await LobbyService.Instance.UpdatePlayerAsync(joinLobby.Id, playerId, new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { TestPlayerData.PLAYER_NAME.GetStringValue(), new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newName) }
                    }
                });

                joinLobby = lobby;
                playerName = newName;

                Debug.Log($"Player changed  name to {newName}");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void KickPlayer(int index)
    {
        try
        {
            if (joinLobby != null && joinLobby.HostId == playerId)
            {
                bool isHost = joinLobby.Players[index].Id == joinLobby.HostId;
                await LobbyService.Instance.RemovePlayerAsync(joinLobby.Id, joinLobby.Players[index].Id);

                Debug.Log($"Player [{index}] has been kicked");
                if (isHost)
                {
                    hostLobby = null;
                    joinLobby = null;
                }
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
    }

    [Command]
    private async void ChangeHost(int index)
    {
        try
        {
            if (joinLobby != null && joinLobby.HostId == playerId)
            {
                var lobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
                {
                    HostId = joinLobby.Players[index].Id
                });

                joinLobby = lobby;
                hostLobby = lobby.HostId == playerId ? lobby : null;

                Debug.Log($"Change host to player [{index}]");
            }
            else
            {
                Debug.Log($"Only host player can change host to other player");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void DeleteLobby()
    {
        try
        {
            if (joinLobby != null && joinLobby.HostId == playerId)
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinLobby.Id);

                Debug.Log($"lobby {joinLobby.Name} : {joinLobby.LobbyCode} has been deleted");

                joinLobby = null;
                hostLobby = null;
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
