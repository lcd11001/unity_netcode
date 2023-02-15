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

    [SerializeField]
    private float heartBeatTimerMax = 15f;

    [SerializeField]
    private string playerName;

    private float heartBeatTimer;
    private string playerId;

    protected async override void Start()
    {
        base.Start();

        heartBeatTimer = heartBeatTimerMax;

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
                        { TestPlayerData.PLAYER_NAME.GetStringValue(), new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                    }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            Debug.Log($"Lobby created [{lobby.Id}] : {lobby.Name} : {lobby.MaxPlayers} : {lobby.LobbyCode}");

            hostLobby = lobby;
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
            foreach (var lobby in response.Results)
            {
                Debug.Log($"{lobby.Name} : {lobby.AvailableSlots} : {lobby.LobbyCode}");
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
    private async void JoinLobbyById(string id)
    {
        try
        {
            var lobby = await Lobbies.Instance.JoinLobbyByIdAsync(id);
            Debug.Log($"Joined to lobby {lobby.Name} : {lobby.AvailableSlots}");

            hostLobby = lobby;
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

            hostLobby = lobby;
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
            var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();
            Debug.Log($"Joined to lobby {lobby.Name} : {lobby.AvailableSlots}");

            hostLobby = lobby;
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
            if (hostLobby != null && !string.IsNullOrEmpty(playerId))
            {
                await LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, playerId);
                Debug.Log($"player {playerId} has leave lobby {hostLobby.Name}");

                hostLobby = null;
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
            Debug.Log($"Players in lobby {lobby.Name}");
            foreach (var player in lobby.Players)
            {
                Debug.Log($"[{player.Id}] {player.Data[TestPlayerData.PLAYER_NAME.GetStringValue()].Value}");
            }
        }
    }

    [Command]
    private void ListPlayers()
    {
        ListPlayers(hostLobby);
    }
}
