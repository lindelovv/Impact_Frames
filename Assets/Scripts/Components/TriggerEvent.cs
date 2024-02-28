using Unity.Entities;
using Unity.Physics;

public struct TriggerEvent : IBufferElementData, ISimulationEvent<TriggerEvent>
{
    public Entity EntityA { get; }
    public Entity EntityB { get; }
    public int BodyIndexA { get; }
    public int BodyIndexB { get; }
    public ColliderKey ColliderKeyA { get; }
    public ColliderKey ColliderKeyB { get; }
    
    public TriggerEvent(TriggerEvent triggerEvent)
    {
        EntityA = triggerEvent.EntityA;
        EntityB = triggerEvent.EntityB;
        BodyIndexA = triggerEvent.BodyIndexA;
        BodyIndexB = triggerEvent.BodyIndexB;
        ColliderKeyA = triggerEvent.ColliderKeyA;
        ColliderKeyB = triggerEvent.ColliderKeyB;
    }
    
    public int CompareTo(TriggerEvent other)
        => ISimulationEventUtilities.CompareEvents(this, other);
}