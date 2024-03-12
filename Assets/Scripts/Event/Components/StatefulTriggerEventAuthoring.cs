using Unity.Assertions;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public class StatefulTriggerEventAuthoring : MonoBehaviour
{
    class Baker : Baker<StatefulTriggerEventAuthoring>
    {
        public override void Bake(StatefulTriggerEventAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddBuffer<StatefulTriggerEvent>(entity);
        }
    }
}

// If this component is added to an entity, trigger events won't be added to a dynamic buffer
// of that entity by the StatefulTriggerEventBufferSystem. This component is by default added to
// CharacterController entity, so that CharacterControllerSystem can add trigger events to
// CharacterController on its own, without StatefulTriggerEventBufferSystem interference.
public struct StatefulTriggerEventExclude : IComponentData {}

/// <summary>
/// Describes an event state.
/// Event state is set to:
///    0) Undefined, when the state is unknown or not needed
///    1) Enter, when 2 bodies are interacting in the current frame, but they did not interact the previous frame
///    2) Stay, when 2 bodies are interacting in the current frame, and they also interacted in the previous frame
///    3) Exit, when 2 bodies are not interacting in the current frame, but they did interact in the previous frame
/// </summary>
public enum StatefulEventState : byte
{
    Undefined,
    Enter,
    Stay,
    Exit
}

/// <summary>
/// Extends ISimulationEvent with extra <see cref="StatefulEventState"/>.
/// </summary>
public interface IStatefulSimulationEvent<T> : IBufferElementData, ISimulationEvent<T>
{
    public StatefulEventState State { get; set; }
}

// Trigger Event that can be stored inside a DynamicBuffer
public struct StatefulTriggerEvent : IStatefulSimulationEvent<StatefulTriggerEvent>
{
    public Entity EntityA           { get; set; }
    public Entity EntityB           { get; set; }
    public int BodyIndexA           { get; set; }
    public int BodyIndexB           { get; set; }
    public ColliderKey ColliderKeyA { get; set; }
    public ColliderKey ColliderKeyB { get; set; }
    public StatefulEventState State { get; set; }

    public StatefulTriggerEvent(TriggerEvent triggerEvent)
    {
        EntityA      = triggerEvent.EntityA;
        EntityB      = triggerEvent.EntityB;
        BodyIndexA   = triggerEvent.BodyIndexA;
        BodyIndexB   = triggerEvent.BodyIndexB;
        ColliderKeyA = triggerEvent.ColliderKeyA;
        ColliderKeyB = triggerEvent.ColliderKeyB;
        State        = default;
    }

    // Returns other entity in EntityPair, if provided with one
    public Entity GetOtherEntity(Entity entity)
    {
        Assert.IsTrue((entity == EntityA) || (entity == EntityB));
        return (entity == EntityA) ? EntityB : EntityA;
    }

    public int CompareTo(StatefulTriggerEvent other) => ISimulationEventUtilities.CompareEvents(this, other);
}
