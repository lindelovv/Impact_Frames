using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.NetCode;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class RelayFrontend : MonoBehaviour
{
    public InputField Address;
    public Button ClientServerButton;

    public string HostConnectionStatus   { get => HostConnectionLabel.text;   set => HostConnectionLabel.text = value;   }
    public string ClientConnectionStatus { get => ClientConnectionLabel.text; set => ClientConnectionLabel.text = value; }

    public Button JoinExistingGame;
    public Text HostConnectionLabel;
    public Text ClientConnectionLabel;

    ConnectionState _state;
    HostServer _hostServerSystem;
    ConnectingPlayer _hostClientSystem;

    enum ConnectionState
    {
        Unknown,
        SetupHost,
        SetupClient,
        JoinGame,
        JoinLocalGame,
    }

    public void Start()
    {
        Address.text = string.Empty;
        Address.placeholder.GetComponent<Text>().text = "join code for host server";
        ClientServerButton.onClick.AddListener(() => { _state = ConnectionState.SetupHost; });
        JoinExistingGame.onClick.AddListener(() => { _state = ConnectionState.SetupClient; });
    }

    private void DestroyLocalSimulationWorld()
    {
        foreach (var world in World.All)
        {
            if (world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }
    }
    
#if !UNITY_SERVER

    public void Update()
    {
        switch (_state)
        {
            case ConnectionState.SetupHost:
            {
                HostServer();
                _state = ConnectionState.SetupClient;
                goto case ConnectionState.SetupClient;
            }
            case ConnectionState.SetupClient:
            {
                var isServerHostedLocally = _hostServerSystem?.RelayServerData.Endpoint.IsValid;
                var enteredJoinCode = !string.IsNullOrEmpty(Address.text);
                if (isServerHostedLocally.GetValueOrDefault())
                {
                    SetupClient();
                    _hostClientSystem.GetJoinCodeFromHost();
                    _state = ConnectionState.JoinLocalGame;
                    goto case ConnectionState.JoinLocalGame;
                }

                if (enteredJoinCode)
                {
                    JoinAsClient();
                    _state = ConnectionState.JoinGame;
                    goto case ConnectionState.JoinGame;
                }
                break;
            }
            case ConnectionState.JoinGame:
            {
                var hasClientConnectedToRelayService = _hostClientSystem?.RelayClientData.Endpoint.IsValid;
                if (hasClientConnectedToRelayService.GetValueOrDefault())
                {
                    ConnectToRelayServer();
                    _state = ConnectionState.Unknown;
                }
                break;
            }
            case ConnectionState.JoinLocalGame:
            {
                var hasClientConnectedToRelayService = _hostClientSystem?.RelayClientData.Endpoint.IsValid;
                if (hasClientConnectedToRelayService.GetValueOrDefault())
                {
                    SetupRelayHostedServerAndConnect();
                    _state = ConnectionState.Unknown;
                }
                break;
            }
            case ConnectionState.Unknown:
            default: return;
        }
    }

    void HostServer()
    {
        var world = World.All[0];
        _hostServerSystem = world.GetOrCreateSystemManaged<HostServer>();
        var enableRelayServerEntity = world.EntityManager.CreateEntity(ComponentType.ReadWrite<EnableRelayServer>());
        world.EntityManager.AddComponent<EnableRelayServer>(enableRelayServerEntity);

        _hostServerSystem.UIBehaviour = this;
        var simGroup = world.GetExistingSystemManaged<SimulationSystemGroup>();
        simGroup.AddSystemToUpdateList(_hostServerSystem);
    }

    void SetupClient()
    {
        var world = World.All[0];
        _hostClientSystem = world.GetOrCreateSystemManaged<ConnectingPlayer>();
        _hostClientSystem.UIBehaviour = this;
        var simGroup = world.GetExistingSystemManaged<SimulationSystemGroup>();
        simGroup.AddSystemToUpdateList(_hostClientSystem);
    }

    void JoinAsClient()
    {
        SetupClient();
        var world = World.All[0];
        var enableRelayServerEntity = world.EntityManager.CreateEntity(ComponentType.ReadWrite<EnableRelayServer>());
        world.EntityManager.AddComponent<EnableRelayServer>(enableRelayServerEntity);
        _hostClientSystem.JoinUsingCode(Address.text);
    }

    void SetupRelayHostedServerAndConnect()
    {
        if (ClientServerBootstrap.RequestedPlayType != ClientServerBootstrap.PlayType.ClientAndServer)
        {
            UnityEngine.Debug.LogError($"Creating client/server worlds is not allowed if playmode is set to {ClientServerBootstrap.RequestedPlayType}");
            return;
        }

        var world = World.All[0];
        var relayClientData = world.GetExistingSystemManaged<ConnectingPlayer>().RelayClientData;
        var relayServerData = world.GetExistingSystemManaged<HostServer>().RelayServerData;
        var joinCode = world.GetExistingSystemManaged<HostServer>().JoinCode;

        var oldConstructor = NetworkStreamReceiveSystem.DriverConstructor;
        NetworkStreamReceiveSystem.DriverConstructor = new RelayDriverConstructor(relayServerData, relayClientData);
        var server = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        NetworkStreamReceiveSystem.DriverConstructor = oldConstructor;

        SceneManager.LoadScene("FrontendHUD");
        SceneManager.LoadScene("RelayHUD", LoadSceneMode.Additive);

        //Destroy the local simulation world to avoid the game scene to be loaded into it
        //This prevent rendering (rendering from multiple world with presentation is not greatly supported)
        //and other issues.
        DestroyLocalSimulationWorld();
        if (World.DefaultGameObjectInjectionWorld == null)
        {
            World.DefaultGameObjectInjectionWorld = server;
        }

        SceneManager.LoadSceneAsync("Main", LoadSceneMode.Additive);
        SceneManager.LoadSceneAsync(4, LoadSceneMode.Additive);

        var joinCodeEntity = server.EntityManager.CreateEntity(ComponentType.ReadOnly<JoinCode>());
        server.EntityManager.SetComponentData(joinCodeEntity, new JoinCode { Value = joinCode });

        var networkStreamEntity = server.EntityManager.CreateEntity(ComponentType.ReadWrite<NetworkStreamRequestListen>());
        server.EntityManager.SetName(networkStreamEntity, "NetworkStreamRequestListen");
        server.EntityManager.SetComponentData(networkStreamEntity, new NetworkStreamRequestListen { Endpoint = NetworkEndpoint.AnyIpv4 });

        networkStreamEntity = client.EntityManager.CreateEntity(ComponentType.ReadWrite<NetworkStreamRequestConnect>());
        client.EntityManager.SetName(networkStreamEntity, "NetworkStreamRequestConnect");
        // For IPC this will not work and give an error in the transport layer. For this sample we force the client to connect through the relay service.
        // For a locally hosted server, the client would need to connect to NetworkEndpoint.AnyIpv4, and the relayClientData.Endpoint in all other cases.
        client.EntityManager.SetComponentData(networkStreamEntity, new NetworkStreamRequestConnect { Endpoint = relayClientData.Endpoint });
    }

    void ConnectToRelayServer()
    {
        var world = World.All[0];
        var relayClientData = world.GetExistingSystemManaged<ConnectingPlayer>().RelayClientData;

        var oldConstructor = NetworkStreamReceiveSystem.DriverConstructor;
        NetworkStreamReceiveSystem.DriverConstructor = new RelayDriverConstructor(new RelayServerData(), relayClientData);
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        NetworkStreamReceiveSystem.DriverConstructor = oldConstructor;

        SceneManager.LoadScene("FrontendHUD");

        //Destroy the local simulation world to avoid the game scene to be loaded into it
        //This prevent rendering (rendering from multiple world with presentation is not greatly supported)
        //and other issues.
        DestroyLocalSimulationWorld();
        if (World.DefaultGameObjectInjectionWorld == null){
            World.DefaultGameObjectInjectionWorld = client;
        }

        SceneManager.LoadSceneAsync("Main", LoadSceneMode.Additive);
        SceneManager.LoadSceneAsync(4, LoadSceneMode.Additive);

        var networkStreamEntity = client.EntityManager.CreateEntity(ComponentType.ReadWrite<NetworkStreamRequestConnect>());
        client.EntityManager.SetName(networkStreamEntity, "NetworkStreamRequestConnect");
        // For IPC this will not work and give an error in the transport layer. For this sample we force the client to connect through the relay service.
        // For a locally hosted server, the client would need to connect to NetworkEndpoint.AnyIpv4, and the relayClientData.Endpoint in all other cases.
        client.EntityManager.SetComponentData(networkStreamEntity, new NetworkStreamRequestConnect { Endpoint = relayClientData.Endpoint });
    }
#endif
}
