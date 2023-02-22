using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Demo
{
    public class UISplash : UIBase<UISplash>
    {
        [SerializeField] float splashTimer = 3f;
        protected override void Start()
        {
            base.Start();

            StartCoroutine(GoToLobbyScene());
        }

        private IEnumerator GoToLobbyScene()
        {
            yield return new WaitForSecondsRealtime(splashTimer);

            // Wait for the loading scene manager to start
            yield return new WaitUntil(() => LoadingSceneManager.Instance != null);

            LoadingSceneManager.Instance.LoadScene(SceneName.DEMO_LOBBY, false);
        }
    }
}
