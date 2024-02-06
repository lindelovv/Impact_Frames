using Unity.Entities;
using Unity.NetCode;

public struct AutoConnect : IComponentData { }

public struct ResetServerOnDisconnect : IComponentData { }

public struct TimeOutServer : IComponentData
{
    public double Value;
}

public readonly partial struct NetworkIdAspect : IAspect
{
    public readonly Entity Self;
    readonly RefRO<NetworkId> _networkId;
    readonly RefRO<NetworkSnapshotAck> _networkSnapshot;
    public int Id => _networkId.ValueRO.Value;
    public int EstimatedRTT => (int) _networkSnapshot.ValueRO.EstimatedRTT;
}
