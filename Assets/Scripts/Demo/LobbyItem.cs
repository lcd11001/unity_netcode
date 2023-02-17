using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class LobbyItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TextMeshProUGUI lobbyName;
    [SerializeField] TextMeshProUGUI lobbyPlayers;
    [SerializeField] TextMeshProUGUI lobbyGameMode;
    [SerializeField] Image lobbyBackground;

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

    public void SetBackgroundColor(Color c)
    {
        lobbyBackground.color = c;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"clicked on {LobbyId}");
    }
}
