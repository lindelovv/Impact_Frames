using System.Collections.Generic;
using System.Linq;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;

public static class RelayUtilities
{
    public static RelayServerEndpoint GetEndpointForConnectionType(List<RelayServerEndpoint> endpoints, string connectionType)
    {
        return enpoints.FirstOrDefault(enpoint => enpoint.connectionType == connectionType);
    }
}

