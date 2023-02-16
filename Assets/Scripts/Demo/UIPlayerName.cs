using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class UIPlayerName : UIBase<UIPlayerName>
    {
        [SerializeField] Button btnPlayerName;
        

        private TextMeshProUGUI playerName;

        protected override void Start()
        {
            base.Start();

            playerName = btnPlayerName.GetComponentInChildren<TextMeshProUGUI>();
            LobbyManager.Instance.PlayerProfile.PlayerName = playerName.text;
        }

        private void OnEnable()
        {
            btnPlayerName.onClick.AddListener(OnUserNameClicked);
            UIDialog.Instance.OnClosed.AddListener(OnDialogClosed);
        }

        private void OnDisable()
        {
            btnPlayerName.onClick.RemoveListener(OnUserNameClicked);
            UIDialog.Instance.OnClosed.RemoveListener(OnDialogClosed);
        }

        private void OnDialogClosed(string text, bool isOK)
        {
            Debug.Log($"text {text} isOK {isOK}");
            if (isOK)
            {
                playerName.text = text;
                LobbyManager.Instance.PlayerProfile.PlayerName = text;
            }
        }

        private void OnUserNameClicked()
        {
            UIDialog.Instance.SetTitle("Change User Name");
            UIDialog.Instance.SetLabel("User Name:");
            UIDialog.Instance.SetPlaceHolder("Input user name here...");
            UIDialog.Instance.Show();
        }
    }
}
