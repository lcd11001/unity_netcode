using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Demo
{
    public class UICreateLobby : UIBase<UICreateLobby>
    {
        [SerializeField] TMP_InputField txtName;
        [SerializeField] TMP_InputField txtMax;
        [SerializeField] Toggle chkPrivate;
        [SerializeField] TMP_Dropdown cboGameMode;
        [SerializeField] Button btnCreate;

        protected override void Start()
        {
            base.Start();
            txtMax.onValidateInput = OnValidNumber;
            txtName.onValidateInput = OnValidAscii;
            
            cboGameMode.ClearOptions();
            cboGameMode.AddOptions(new List<TMP_Dropdown.OptionData> 
            {
                new TMP_Dropdown.OptionData(GAME_MODE.GLOBAL.GetStringValue()),
                new TMP_Dropdown.OptionData(GAME_MODE.TENNIS.GetStringValue())
            });
        }

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
            LobbyManager.Instance.CreateLobby(txtName.text, int.Parse(txtMax.text), chkPrivate.isOn, cboGameMode.options[cboGameMode.value].text);
        }
    }
}
