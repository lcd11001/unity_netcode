using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class UIAuthen : UIBase<UIAuthen>
    {
        [SerializeField] Button btnLogin;

        private void OnEnable()
        {
            btnLogin.onClick.AddListener(OnLoginClicked);
        }

        private void OnDisable()
        {
            btnLogin.onClick.RemoveListener(OnLoginClicked);
        }

        private void OnLoginClicked()
        {
            LobbyManager.Instance.Authen();
            this.Hide();
        }
    }
}
