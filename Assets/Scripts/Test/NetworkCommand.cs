using SmartConsole;
using SmartConsole.Components;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkCommand : CommandBehaviour
{
    ConsoleUIGenerator consoleUIGenerator;
    
    protected override void Start()
    {
        base.Start();
        consoleUIGenerator = FindObjectOfType<ConsoleUIGenerator>();

        Debug.LogWarning("Type \"help\" to list all commands");
    }

    [Command]
    public void help()
    {
        for (int i = 0; i < Command.List.Count; i++)
        {
            if (Command.List[i].MethodInfo.Name != "help")
            {
                Debug.Log(Command.List[i].MethodInfo.Name);
            }
        }
    }

    [Command]
    public void set_background_alpha(float alpha)
    {
        if (consoleUIGenerator != null)
        {
            consoleUIGenerator.BackgroundAlpha = alpha;
        }
    }

    [Command]
    public void start_host()
    {
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("Host started");
        }
        else
        {
            Debug.Log("Host could not started");
        }
    }

    [Command]
    public void start_server()
    {
        if (NetworkManager.Singleton.StartServer())
        {
            Debug.Log("Server started");
        }
        else
        {
            Debug.Log("Server could not started");
        }
    }

    [Command]
    public void start_client()
    {
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client started");
        }
        else
        {
            Debug.Log("Client could not started");
        }
    }
}
