using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public struct SpawnPlayerRequest : IRpcCommand { }

public readonly partial struct NetworkIdAspect : IAspect
{
    public readonly Entity Self;
    readonly RefRO<NetworkId> NetworkId;
    readonly RefRO<NetworkSnapshotAck> NetworkSnapshot;
    
    public int Id => NetworkId.ValueRO.Value;
    public int EstimatedRTT => (int)NetworkSnapshot.ValueRO.EstimatedRTT;
}
