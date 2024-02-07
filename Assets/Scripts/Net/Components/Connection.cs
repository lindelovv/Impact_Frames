using Unity.Entities;
using Unity.NetCode;

public readonly partial struct ConnectionAspect : IAspect
{
    public readonly Entity Self;
    readonly RefRO<NetworkId> _networkId;
    readonly RefRO<NetworkSnapshotAck> _networkSnapshot;

    public int Id => _networkId.ValueRO.Value;
    public int EstimatedRTT => (int)_networkSnapshot.ValueRO.EstimatedRTT;
}

public struct ResetServerOnDisconnect : IComponentData {}

public struct GoInGameRPC : IRpcCommand {}

