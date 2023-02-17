using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class LobbyItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI lobbyName;
    [SerializeField] TextMeshProUGUI lobbyPlayers;
    [SerializeField] TextMeshProUGUI lobbyGameMode;

    [field: SerializeField] public string LobbyId { get; set; } = string.Empty;

    public void SetLobbyName(string name)
    {
        lobbyName.text = name;
    }

    public void SetLobbyPlayers(string players)
    {
        lobbyPlayers.text = players;
    }

    public void SetLobbyGameMode(string gameMode)
    {
        lobbyGameMode.text = gameMode;
    }
}
