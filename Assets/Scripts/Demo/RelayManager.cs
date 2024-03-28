using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Demo
{
    [RequireComponent(typeof(NetworkManager))]
    [RequireComponent(typeof(UnityTransport))]
    public class RelayManager : MonoBehaviourSingletonPersistent<RelayManager>
    {
        [SerializeField] private string environment = "staging"; //"production";

        public bool IsRelayEnabled => Transport != null && Transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport;

        public UnityTransport Transport => NetworkManager.Singleton.NetworkConfig.NetworkTransport.gameObject.GetComponent<UnityTransport>();

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => NetworkManager.Singleton != null);
            if (!IsRelayEnabled)
            {
                Debug.LogWarning("Please switch to Relay Unity Transport");
            }
        }

        public async Task<string> CreateRelay(int maxConnections)
        {
            try
            {
                Debug.Log($"CreateRelay begin environment {environment} with max connection {maxConnections}");
                InitializationOptions options = new InitializationOptions()
                    .SetEnvironmentName(environment);

                await UnityServices.InitializeAsync(options);

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log($"Signed in with user ID {AuthenticationService.Instance.PlayerId}");
                }

                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
                Debug.Log($"Host allocation ID {allocation.AllocationId} region {allocation.Region}");

                string JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
                Debug.LogWarning($"Relay server: {relayServerData.Endpoint.ToString()}");

                Transport.SetRelayServerData(relayServerData);

                Debug.Log($"CreateRelay end with join code {JoinCode}");
                return JoinCode;
            }
            catch (RelayServiceException e)
            {
                Debug.Log($"SetupRelay error {e}");
            }

            return default(string);
        }

        public async Task<bool> JoinRelay(string joinCode)
        {
            try
            {
                Debug.Log($"Client joining .. with join code {joinCode}");
                InitializationOptions options = new InitializationOptions()
                    .SetEnvironmentName(environment);

                await UnityServices.InitializeAsync(options);

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log($"Signed in with user ID {AuthenticationService.Instance.PlayerId}");
                }

                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                Debug.Log($"Client join allocation ID {joinAllocation.AllocationId}");

                RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
                Debug.LogWarning($"Relay server: {relayServerData.Endpoint.ToString()}");

                Transport.SetRelayServerData(relayServerData);

                Debug.Log($"Client joined game with join code {joinCode}");

                return true;
            }
            catch (RelayServiceException e)
            {
                Debug.Log($"JoinRelay error {e}");
            }

            return false;
        }
    }
}
