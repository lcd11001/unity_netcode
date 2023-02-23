using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Demo
{
    public class GameManager : NetworkBehaviourSingleton<GameManager>
    {
        [SerializeField] GameObject playerPrefab;
        
        public void ServerSceneInit(ulong clientId)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(-3.0f, 3.0f),
                0,
                Random.Range(-3.0f, 3.0f)
            );
            GameObject go = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            NetworkObject no = go.GetComponent<NetworkObject>();
            no.SpawnWithOwnership(clientId, true);
        }

        public void QuitGame()
        {
            if (IsServer)
            {
                StartCoroutine(HostShutdown());
            }
            else
            {
                Shutdown();
            }
        }

        private void Shutdown()
        {
            Debug.Log($"Shutdown");
            NetworkManager?.Shutdown();
            LoadingSceneManager.Instance?.LoadScene(SceneName.DEMO_LOBBY, false);
        }

        private IEnumerator HostShutdown()
        {
            Debug.Log($"HostShutdown");
            ShutdownClientRpc();

            yield return new WaitForSeconds(0.5f);

            Shutdown();
        }

        [ClientRpc]
        private void ShutdownClientRpc()
        {
            Debug.Log($"ShutdownClientRpc");
            if (IsServer)
            {
                return;
            }

            Shutdown();
        }
    }
}
