using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport.Relay;

public class RelayDriverConstructor : INetworkStreamDriverConstructor
{
    RelayServerData _relayServerData;
    RelayServerData _relayClientData;

    public RelayDriverConstructor(RelayServerData serverData, RelayServerData clientData)
    {
        _relayServerData = serverData;
        _relayClientData = clientData;
    }

    public void CreateClientDriver(World world, ref NetworkDriverStore driverStore, NetDebug netDebug)
    {
        var settings = DefaultDriverBuilder.GetNetworkSettings();
        settings.WithRelayParameters(ref _relayClientData);
        DefaultDriverBuilder.RegisterClientUdpDriver(world, ref driverStore, netDebug, settings);
    }

    public void CreateServerDriver(World world, ref NetworkDriverStore driverStore, NetDebug netDebug)
    {
        DefaultDriverBuilder.RegisterServerDriver(world, ref driverStore, netDebug, ref _relayServerData);
    }
}

