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
            LobbyManager.Instance.PlayerProfile.Name = playerName.text;
        }

        private void OnEnable()
        {
            btnPlayerName.onClick.AddListener(OnUserNameClicked);
            UIDialog.Instance.OnClosed.AddListener(OnDialogClosed);

            LobbyManager.Instance.OnAuthenSignedIn.AddListener(OnAuthenSignedIn);
        }

        private void OnDisable()
        {
            btnPlayerName.onClick.RemoveListener(OnUserNameClicked);
            UIDialog.Instance.OnClosed.RemoveListener(OnDialogClosed);

            LobbyManager.Instance.OnAuthenSignedIn.RemoveListener(OnAuthenSignedIn);
        }

        private void OnAuthenSignedIn()
        {
            this.Hide();
        }

        private void OnDialogClosed(string text, bool isOK)
        {
            if (isOK)
            {
                playerName.text = text;
                LobbyManager.Instance.PlayerProfile.Name = text;
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
