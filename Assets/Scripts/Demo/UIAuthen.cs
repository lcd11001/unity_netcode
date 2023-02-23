using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class UIAuthen : UIBase<UIAuthen>
    {
        [SerializeField] Toggle chkRelay;
        [SerializeField] Button btnLogin;

        protected override void Start()
        {
            base.Start();

            // refresh value after back from in-game
            OnRelayChanged(chkRelay.isOn);
        }

        private void OnEnable()
        {
            btnLogin.onClick.AddListener(OnLoginClicked);
            chkRelay.onValueChanged.AddListener(OnRelayChanged);
        }

        private void OnDisable()
        {
            btnLogin.onClick.RemoveListener(OnLoginClicked);
            chkRelay.onValueChanged.RemoveListener(OnRelayChanged);
        }

        private void OnRelayChanged(bool enable)
        {
            chkRelay.GetComponentInChildren<Text>().text = enable ? "Relay" : "127.0.0.1:7777";
            if (enable)
            {
                RelayManager.Instance.Transport.SetRelayServerData(default(RelayServerData));
            }
            else
            {
                RelayManager.Instance.Transport.SetConnectionData("127.0.0.1", 7777);
            }

            StartCoroutine(RefreshLayout(chkRelay));
        }

        private IEnumerator RefreshLayout(Toggle toggle)
        {
            yield return new WaitForEndOfFrame();
            var layout = toggle.GetComponent<HorizontalLayoutGroup>();
            layout.enabled = false;
            layout.enabled = true;
        }

        private void OnLoginClicked()
        {
            LobbyManager.Instance.Authen();
            this.Hide();
        }
    }
}
