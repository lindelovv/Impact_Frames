using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.NetCode;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;
using UnityEngine.Serialization;

public class Frontend : MonoBehaviour
{
    const ushort k_NetworkPort = 7979;

    public InputField Address;
    public InputField Port;
    public Button ClientServerButton;

    /// <summary>
    /// Stores the old name of the local world (create by initial bootstrap).
    /// It is reused later when the local world is created when coming back from game to the menu.
    /// </summary>
    internal static string OldFrontendWorldName = string.Empty;

    public void Start()
    {
        ClientServerButton.gameObject.SetActive(ClientServerBootstrap.RequestedPlayType == ClientServerBootstrap.PlayType.ClientAndServer);
    }

    public void StartClientServer(string sceneName)
    {
        if (ClientServerBootstrap.RequestedPlayType != ClientServerBootstrap.PlayType.ClientAndServer)
        {
            Debug.LogError($"Creating client/server worlds is not allowed if playmode is set to {ClientServerBootstrap.RequestedPlayType}");
            return;
        }

        var server = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");

        SceneManager.LoadScene("FrontendHUD");

        //Destroy the local simulation world to avoid the game scene to be loaded into it
        //This prevent rendering (rendering from multiple world with presentation is not greatly supported)
        //and other issues.
        DestroyLocalSimulationWorld();
        if (World.DefaultGameObjectInjectionWorld == null)
            World.DefaultGameObjectInjectionWorld = server;
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        var port = ParsePortOrDefault(Port.text);

        NetworkEndpoint ep = NetworkEndpoint.AnyIpv4.WithPort(port);
        {
            using var drvQuery = server.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(ep);
        }

        ep = NetworkEndpoint.LoopbackIpv4.WithPort(port);
        {
            using var drvQuery = client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, ep);
        }
    }

    //StartClientServer("Main");

    public void ConnectToServer()
    {
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        SceneManager.LoadScene("FrontendHUD");
        DestroyLocalSimulationWorld();
        if (World.DefaultGameObjectInjectionWorld == null)
        {
            
            World.DefaultGameObjectInjectionWorld = client;
        }
        SceneManager.LoadSceneAsync("Main", LoadSceneMode.Additive);

        var ep = NetworkEndpoint.Parse(Address.text, ParsePortOrDefault(Port.text));
        {
            using var drvQuery = client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, ep);
        }
    }

    protected void DestroyLocalSimulationWorld()
    {
        foreach (var world in World.All)
        {
            if (world.Flags == WorldFlags.Game)
            {
                OldFrontendWorldName = world.Name;
                world.Dispose();
                break;
            }
        }
    }

    // Tries to parse a port, returns true if successful, otherwise false
    // The port will be set to whatever is parsed, otherwise the default port of k_NetworkPort
    private UInt16 ParsePortOrDefault(string s)
    {
        if (!UInt16.TryParse(s, out var port))
        {
            Debug.LogWarning($"Unable to parse port, using default port {k_NetworkPort}");
            return k_NetworkPort;
        }

        return port;
    }
}
