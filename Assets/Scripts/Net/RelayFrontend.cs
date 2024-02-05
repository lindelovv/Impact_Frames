#if !UNITY_SERVER
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RelayFrontend : Frontend
{
    public string HostConnectionStatus   { get => HostConnectionLabel.text;   set => HostConnectionLabel.value;   }
    public string ClientConnectionStatus { get => ClientConnectionLabel.text; set => ClientConnectionLabel.value; }
    public Button JoinExistingGame;
    public Text HostConnectionLabel;
    public Text ClientConnectionLabel;

    string _oldValue;
    ConnectionState _state;
    HostServerSystem _hostServerSystem;
    ConnectingPlayerSystem _hostClientSystem;

    enum ConnectionState
    {
        Unknown,
        SetupHost,
        SetupClient,
        JoinGame,
        JoinLocalGame,
    }

    public void OnRelayEnable(Toggle value)
    {
        TogglePersistentState(!value.isOn);
        if(value.isOn)
        {
            Port.gameObject.SetActive(false);
            _oldValue = Address.text;
            Adress.text = string.Empty;
            Address.placeholder.GetComponent<Text>().text = "Join Code for Host Server";
            ClientServerButton.onClick.AddListener(() => { _state = ConnectionState.SetupHost; });
            JoinExistingGame.onClick.AddListener(() => { _state = ConnectionState.SetupClient; });
        }
        else 
        {
            Port.gameObject.SetActive(true);
            Address.text = _oldValue;
            Address.placeholder.GetComponent<Text>().text = string.Empty;
            ClientServerButton.onClick.RemoveAllListeners();
            JoinExistingGame.onClick.RemoveAllListeners();
        }
    }

    void TogglePersistentState(bool shouldListen)
    {
        if(shouldListen)
        {
            ClientServerButton.onClick.SetPersistentListenerState(0, UnityEventCallState.RuntimeOnly);
            JoinExistingGame.onClick.SetPersistentListenerState(0, UnityEventCallState.RuntimeOnly);
        }
        else
        {
            ClientServerButton.onClick.SetPersistentListenerState(0, UnityEventCallState.Off);
            JoinExistingGame.onClick.SetPersistentListenerState(0, UnityEventCallState.Off);
        }
    }

    public void Update()
    {
        switch(_state)
        {
        //-------------------------------------------
            case ConnectionState.SetupHost:
            {
                HostServer();
                _state = ConnectionState.SetupClient;
                goto case ConnectionState.SetupClient;
            }
        //-------------------------------------------
            case ConnectionState.SetupClient:
            {
                var isServerHostedLocally = _hostServerSystem?.RelayServerData.Endpoint.IsValid;
                var enteredJoinCode = !string.IsNullOrEmpty(Address.text);
                if(isServerHostedLocally.GetValueOrDefault())
                {
                    SetupClient();
                    _hostClientSystem.GetJoinCodeFromHost();
                    _state = ConnectionState.JoinLocalGame;
                    goto case ConnectionState.JoinLocalGame;
                }
                if(enteredJoinCode)
                {
                    JoinAsClient();
                    _state = ConnectionState.JoinGame;
                    goto case ConnectionState.JoinGame;
                }
                break;
            }
        //-------------------------------------------
            case ConnectionState.JoinGame:
            {
                var hasClientConnectedToRelayService = _hostClientSystem?.RelayClientData.Endpoint.IsValid;
                if(hasClientConnectedToRelayService.GetValueOrDefault())
                {
                    ConnectToRelayServer();
                    _state = ConnectionState.Unknown;
                }
                break;
            }
        //-------------------------------------------
            case ConnectionState.JoinLocalGame:
            {
                var hasClientConnectedToRelayService = _hostClientSystem?.RelayClientData.Endpoint.IsValid;
                if(hasClientConnectedToRelayService.GetValueOrDefault())
                {
                    SetupRelayHostedServerAndConnect();
                    _state = ConnectionState.Unknown;
                }
                break;
            }
        //-------------------------------------------
            case ConnectionState.Unknown:
            default:
            return;
        }
    }

    void HostServer()
    {
        var world = World.All[0];
        _hostServerSystem = world.GetOrCreateSystemManaged<HostServerSystem>();
        var enableRelayServerEntity = world.EntityManager.CreateEntity(ComponentType.ReadWrite<EnableRelayServerTag>());
        world.EntityManager.AddComponent<EnableRelayServerTag>(enableRelayServerEntity);

        _hostServerSystem.UIBehaviour = this;
        var simGroup = world.GetExistingSystemManged<SimulationSystemGroup>();
        simGroup.AddSystemToUpdateList(_hostServerSystem);
    }

    void SetupClient()
    {
        var world = World.All[0];
        _hostClientSystem = world.GetOrCreateSystemManaged<ConnectingPlayerSystem>();
        _hostClientSystem.UIBehaviour = this;
        var simGroup = world.GetExistingSystemManged<SimulationSystemGroup>();
        simGroup.AddSystemToUpdateList(_hostClientSystem);
    }

    void JoinAsClient()
    {
        SetupClient();
        var world = World.All[0];
        var enableRelayServerEntity = world.EntityManager.CreateEntity(ComponentType.ReadWrite<EnableRelayServerTag>());
        world.EntityManager.AddComponent<EnableRelayServerTag>(enableRelayServerEntity);
        _hostClientSystem.JoingUsingCode(Address.text);
    }

    void SetupRelayHostedServerAndConnect()
    {
        if(ClientServerBootstap.RequestedPlayType != ClientServerBootstap.PlayType.ClientAndServer)
        {
            UnityEngine.Debug.LogError($"Creating client/server worlds is not allowed if playmode is set to {ClientServerBootstap.RequestedPlayType}");
            return;
        }

        var world = World.All[0];
        var relayClientData = world.GetExistingSystemManged<ConnectingPlayerSystem>().RelayClientData;
        var relayServerData = world.GetExistingSystemManged<HostServerSystem>().RelayServerData;
        var joinCode = world.GetExistingSystemManged<HostServerSystem>().JoinCode;

        var oldConstructor = NetworkStreamReceiveSystem.DriverConstructor;
        NetworkStreamReceiveSystem.DriverConstructor = new RelayDriverConstructor(relayServerData, relayClientData);
        var server = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        NetworkStreamReceiveSystem.DriverConstructor = oldConstructor;

        SceneManager.LoadScene("FrontendHUD");
        SceneManager.LoadScene("RelayHUD", LoadSceneMode.Additive);

        DestroyLocalSimulationWorld();
        if(World.DefaultGameObjectInjectionWorld == null)
        {
            World.DefaultGameObjectInjectionWorld = server;
        }

        SceneManager.LoadSceneAsync(GetAndSaveSceneSelection(), LoadSceneMode.Addative);

        var joinCodeEntity = server.EntityManager.CreateEntity(ComponentType.ReadOnly<JoinCode>());
        server.EntityManager.SetComponentData(joinCodeEntity, new JoinCode { Value = joinCode });

        var networkStreamEntity = server.EntityManager.CreateEntity(ComponentType.ReadWrite<NetworkStreamRequestListen>());
        server.EntityManager.SetName(networkStreamEntity, "NetworkStreamRequestListen");
        server.EntityManager.SetComponentData(networkStreamEntity, new NetworkStreamRequestListen { Endpoint = NetworkEndpoint.AnyIpv4 });

        networkStreamEntity = client.EntityManager.CreateEntity(ComponentType.ReadWrite<NetworkStreamRequestConnect>());
        client.EntityManager.SetName(networkStreamEntity, "NetworkStreamRequstConnect");
        client.EntityManager.SetComponentData(networkStreamEntity, new NetworkStreamRequestConnect { Endpoint = relayClientData.Endpoint });
    }

    void ConnectToRelayServer()
    {
        var world = World.All[0];
        var relayClientData = world.GetExistingSystemManged<ConnectingPlayerSystem>().RelayClientData;

        var oldConstructor = NetworkStreamRecieveSystem.DriverConstructor;
        NetworkStreamReceiveSystem.DriverConstructor = new RelayDriverConstructor(new RelayServerData(), relayClientData);
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        NetworkStreamReceiveSystem.DriverConstructor = oldConstructor;

        SceneManager.LoadScene("FrontendHUD");

        DestroyLocalSimulationWorld();
        if(World.DefaultGameObjectInjectionWorld == null)
        {
            World.DefaultGameObjectInjectionWorld = client;
        }

        SceneManager.LoadSceneAsync(GetAndSaveSceneSelection(), LoadSceneMode.Additive);

        networkStreamEntity = client.EntityManager.CreateEntity(ComponentType.ReadWrite<NetworkStreamRequestConnect>());
        client.EntityManager.SetName(networkStreamEntity, "NetworkStreamRequestConnect");
        client.EntityManager.SetComponentData(networkStreamEntity, new NetworkStreamRequestConnect { Endpoint = relayClientData.Endpoint });
    }
}

#endif
