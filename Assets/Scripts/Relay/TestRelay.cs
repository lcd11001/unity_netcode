using SmartConsole;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class TestRelay : CommandBehaviour
{
    protected override async void Start()
    {
        base.Start();
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"Signed in {AuthenticationService.Instance.PlayerId}");
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    [Command]
    private async void CreateRelay(int maxPlayers)
    {
        try
        {
            // you will be the host, so that minus 1
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"joinCode {joinCode}");

            //NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
            //    ipAddress: allocation.RelayServer.IpV4,
            //    port: (ushort)allocation.RelayServer.Port,
            //    allocationId: allocation.AllocationIdBytes,
            //    key: allocation.Key,
            //    connectionData: allocation.ConnectionData,
            //    isSecure: false
            //);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log($"Joined relay successfull with code {joinCode} ");

            //NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
            //    ipAddress: allocation.RelayServer.IpV4,
            //    port: (ushort)allocation.RelayServer.Port,
            //    allocationId: allocation.AllocationIdBytes,
            //    key: allocation.Key,
            //    connectionData: allocation.ConnectionData,
            //    hostConnectionData: allocation.HostConnectionData, // <= client
            //    isSecure: false
            //);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
