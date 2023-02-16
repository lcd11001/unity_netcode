using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Demo
{
    public class LobbyManager : MonoBehaviourSingletonPersistent<LobbyManager>
    {
        private string userName;

        private void OnSignedIn()
        {
            Debug.Log($"{this.userName} signed in");
            UILoading.Instance.Hide();
        }

        public async void Authen(string userName)
        {
            UILoading.Instance.Show();

            this.userName = userName;

            var initOption = new InitializationOptions()
                .SetProfile(userName);

            await UnityServices.InitializeAsync(initOption);

            AuthenticationService.Instance.SignedIn += OnSignedIn;

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
}
