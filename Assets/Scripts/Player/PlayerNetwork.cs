using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    private NetworkVariable<int> randomNumber = new NetworkVariable<int>(
        value: 1,
        writePerm: NetworkVariableWritePermission.Owner
    );

    struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        // Can not use "string" here because it's nullable type
        public FixedString128Bytes _string;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref _string);
        }

        public override string ToString()
        {
            return $"{{int: {_int} bool: {_bool} string: {_string}}}";
        }
    }

    private NetworkVariable<MyCustomData> customData = new NetworkVariable<MyCustomData>(
        value: new MyCustomData { _int = 1, _bool = true, _string = "" },
        writePerm: NetworkVariableWritePermission.Owner
    );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            Debug.Log("My ID " + OwnerClientId);
        }
        // register network callback inside OnNetworkSpawn
        // do not in Awake or Start
        randomNumber.OnValueChanged += OnRandomNumberChanged;
        customData.OnValueChanged += OnCustomDataChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        randomNumber.OnValueChanged -= OnRandomNumberChanged;
        customData.OnValueChanged -= OnCustomDataChanged;
    }

    private void OnCustomDataChanged(MyCustomData previousValue, MyCustomData newValue)
    {
        Debug.Log($"[{OwnerClientId}] custom data prev {previousValue} new {newValue}");
    }

    private void OnRandomNumberChanged(int previousValue, int newValue)
    {
        Debug.Log($"[{OwnerClientId}] random number prev {previousValue} new {newValue}");
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            randomNumber.Value = Random.Range(0, 100);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            // TestServerRpc(RandomString(128).ToString());
            // TestParamServerRpc(new ServerRpcParams());
            // TestClientRpc();

            if (IsServer && NetworkManager.ConnectedClients.ContainsKey(1))
            {
                TestParamClientRpc(Random.Range(0, 100), new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] {
                            // 0,
                            1
                        }
                    }
                });
            }
            /**
            customData.Value = new MyCustomData
            {
                _int = Random.Range(0, 100),
                _bool = Random.Range(0, 2) == 0,
                _string = RandomString(FixedString128Bytes.UTF8MaxLengthInBytes)
            };
            */
        }

        Vector3 moveDir = new Vector3(0, 0, 0);
        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private FixedString128Bytes RandomString(int maxLen)
    {
        int len = Random.Range(1, maxLen);
        StringBuilder sb = new StringBuilder(maxLen);
        for (int i = 0; i < len; i++)
        {
            sb.Append(System.Convert.ToChar(Random.Range((int)'A', (int)'Z' + 1)));
        }
        return sb.ToString();
    }

    [ServerRpc]
    private void TestServerRpc(string message)
    {
        // This code call from client but only run on server
        Debug.Log("TestServerRpc " + OwnerClientId + " : " + message);
        // customData.Value = new MyCustomData
        // {
        //     _int = Random.Range(0, 100),
        //     _bool = Random.Range(0, 2) == 0,
        //     _string = RandomString(FixedString128Bytes.UTF8MaxLengthInBytes)
        // };
    }

    [ServerRpc]
    private void TestParamServerRpc(ServerRpcParams param)
    {
        // This code call from client but only run on server
        Debug.Log("TestParamServerRpc ownerID " + OwnerClientId + " senderID " + param.Receive.SenderClientId);
    }

    [ClientRpc]
    private void TestClientRpc()
    {
        // This code call from server but run on all clients
        Debug.Log("TestClientRpc ownerID " + OwnerClientId);
    }

    [ClientRpc]
    private void TestParamClientRpc(int number, ClientRpcParams param)
    {
        // This code call from server but run on all clients
        Debug.Log("TestClientRpc ownerID " + OwnerClientId + " number " + number);
    }
}
