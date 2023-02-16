using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class UILogin : UIBase<UILogin>
    {
        [SerializeField] Button btnUserName;
        [SerializeField] Button btnLogin;

        private TextMeshProUGUI userName;

        protected override void Start()
        {
            base.Start();

            userName = btnUserName.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            btnUserName.onClick.AddListener(OnUserNameClicked);
            btnLogin.onClick.AddListener(OnLoginClicked);
            UIDialog.Instance.OnClosed.AddListener(OnDialogClosed);
        }

        private void OnDisable()
        {
            btnUserName.onClick.RemoveListener(OnUserNameClicked);
            btnLogin.onClick.RemoveListener(OnLoginClicked);
            UIDialog.Instance.OnClosed.RemoveListener(OnDialogClosed);
        }

        private void OnDialogClosed(string text, bool isOK)
        {
            Debug.Log($"text {text} isOK {isOK}");
            if (isOK)
            {
                userName.text = text;
            }
        }

        private void OnUserNameClicked()
        {
            UIDialog.Instance.SetTitle("Change User Name");
            UIDialog.Instance.SetLabel("User Name:");
            UIDialog.Instance.SetPlaceHolder("Input user name here...");
            UIDialog.Instance.Show();
        }

        private void OnLoginClicked()
        {
            LobbyManager.Instance.Authen(this.userName.text);
            this.Hide();
        }
    }
}
