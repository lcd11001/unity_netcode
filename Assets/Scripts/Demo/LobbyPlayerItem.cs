using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Demo
{
    public class LobbyPlayerItem : MonoBehaviour
    {
        [SerializeField] Image playerIcon;
        [SerializeField] TextMeshProUGUI playerName;
        [SerializeField] Button btnKick;
        [field: SerializeField] public string PlayerId;

        private void OnEnable()
        {
            btnKick.onClick.AddListener(OnKickPlayer);
        }

        private void OnDisable()
        {
            btnKick.onClick.RemoveListener(OnKickPlayer);
        }

        public void EnableKickButton(bool enable)
        {
            btnKick.gameObject.SetActive(enable);
        }

        public void SetPlayerName(string name)
        {
            playerName.text = name;
        }

        public void SetPlayerIcon(Sprite icon)
        {
            playerIcon.sprite = icon;
        }

        private async void OnKickPlayer()
        {
            bool isOK = await LobbyManager.Instance.KickPlayer(PlayerId);
            if (isOK)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
