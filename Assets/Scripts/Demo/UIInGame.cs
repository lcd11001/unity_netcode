using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class UIInGame : UIBase<UIInGame>
    {
        [SerializeField] Button btnQuit;

        private void OnEnable()
        {
            btnQuit.onClick.AddListener(OnQuitClicked);
        }

        private void OnDisable()
        {
            btnQuit.onClick.RemoveListener(OnQuitClicked);
        }

        private void OnQuitClicked()
        {
            GameManager.Instance.QuitGame();
        }
    }
}
