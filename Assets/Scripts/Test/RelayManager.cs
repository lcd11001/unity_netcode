using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private string environment = "production";
    [SerializeField] private int maxConnections = 50;

    public bool IsRelayEnabled => Transport != null && Transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport;

    public UnityTransport Transport => NetworkManager.Singleton.NetworkConfig.NetworkTransport.gameObject.GetComponent<UnityTransport>();

    public async Task<RelayHostData> SetupRelay()
    {
        Debug.Log($"SetupRelay begin environment {environment} with max connection {maxConnections}");
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

        RelayHostData relayHostData = new RelayHostData
        {
            PlayerID = AuthenticationService.Instance.PlayerId,
            Key = allocation.Key,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            IPv4Address = allocation.RelayServer.IpV4,
            ConnectionData = allocation.ConnectionData
        };

        relayHostData.JoinCode = await RelayService.Instance.GetJoinCodeAsync(relayHostData.AllocationID);

        Transport.SetRelayServerData(relayHostData.IPv4Address, relayHostData.Port, relayHostData.AllocationIDBytes, relayHostData.Key, relayHostData.ConnectionData);

        Debug.Log($"SetupRelay end with join code {relayHostData.JoinCode}");
        return relayHostData;
    }

    public async Task<RelayJoinData> JoinRelay(string joinCode)
    {
        Debug.Log($"Client joining with join code {joinCode}");
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

        RelayJoinData relayJoinData = new RelayJoinData
        {
            PlayerID = AuthenticationService.Instance.PlayerId,
            Key = joinAllocation.Key,
            Port = (ushort)joinAllocation.RelayServer.Port,
            AllocationID = joinAllocation.AllocationId,
            AllocationIDBytes = joinAllocation.AllocationIdBytes,
            IPv4Address = joinAllocation.RelayServer.IpV4,
            ConnectionData = joinAllocation.ConnectionData,
            HostConnectionData = joinAllocation.HostConnectionData,
            JoinCode = joinCode
        };

        Transport.SetRelayServerData(relayJoinData.IPv4Address, relayJoinData.Port, relayJoinData.AllocationIDBytes, relayJoinData.Key, relayJoinData.ConnectionData, relayJoinData.HostConnectionData);

        Debug.Log($"Client joined game with join code {relayJoinData.JoinCode}");

        return relayJoinData;
    }
}

public struct RelayHostData
{
    public string PlayerID;
    public string JoinCode;
    public string IPv4Address;
    public ushort Port;
    public Guid AllocationID;
    public byte[] AllocationIDBytes;
    public byte[] ConnectionData;
    public byte[] Key;
}

public struct RelayJoinData
{
    public string PlayerID;
    public string JoinCode;
    public string IPv4Address;
    public ushort Port;
    public Guid AllocationID;
    public byte[] AllocationIDBytes;
    public byte[] ConnectionData;
    public byte[] Key;
    public byte[] HostConnectionData;
}
