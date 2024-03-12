using Unity.Entities;
using Unity.Physics;

public struct CustomTriggerEvent : IBufferElementData, ISimulationEvent<CustomTriggerEvent>
{
    public Entity EntityA { get; }
    public Entity EntityB { get; }
    public int BodyIndexA { get; }
    public int BodyIndexB { get; }
    public ColliderKey ColliderKeyA { get; }
    public ColliderKey ColliderKeyB { get; }
    
    public CustomTriggerEvent(TriggerEvent triggerEvent)
    {
        EntityA = triggerEvent.EntityA;
        EntityB = triggerEvent.EntityB;
        BodyIndexA = triggerEvent.BodyIndexA;
        BodyIndexB = triggerEvent.BodyIndexB;
        ColliderKeyA = triggerEvent.ColliderKeyA;
        ColliderKeyB = triggerEvent.ColliderKeyB;
    }
    
    public int CompareTo(CustomTriggerEvent other)
        => ISimulationEventUtilities.CompareEvents(this, other);
}
