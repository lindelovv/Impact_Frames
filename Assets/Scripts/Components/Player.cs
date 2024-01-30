using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

public class Player : MonoBehaviour
{
    [Header("Health")] 
    [Tooltip("[float] Remaining health points.")] public float CurrentHealth;
    [Tooltip("[float] The maximum health possible.")] public float MaxHealth;
    
    [Header("Movement")] 
    [Tooltip("[float] How fast the player can move.")] public float Speed;
    [Tooltip("[float] How high the player can jump.")] public float JumpHeight;
    
    [Header("Physics")] 
    [Tooltip("[float3] Strength of the downwards pull.")] public float3 Gravity;
    [Tooltip("[float] Movement resistance.")] public float AirResistance;
    [Tooltip("[float3] Override starting velocity. Normally should be {0,0,0}.")] public float3 StartingVelocity;
    [Tooltip("[float] Top velocity.")] public float TerminalVelocity;
    [Tooltip("[float] Stationary friction.")] public float StaticFriction;
    [Tooltip("[float] Movement friction.")] public float KineticFriction;

    public class Baker : Baker<Player>
    {
        public override void Bake(Player authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new PlayerData {
                MovementSpeed = authoring.Speed,
                JumpHeight    = authoring.JumpHeight,
            });
            
            // Health component is it's own component in case of reuse,
            // but since we always want one on the player it is added here in code
            // from the values from this authoring component (see [Header("Health")] above).
            AddComponent(entity, new HealthComponent {
                CurrentHealth = authoring.CurrentHealth,
                MaxHealth     = authoring.MaxHealth,
            });
            
            // Same as above
            AddComponent(entity, new VelocityComponent {
                CurrentVelocity  = authoring.StartingVelocity,
                TerminalVelocity = authoring.TerminalVelocity,
                AirResistance = authoring.AirResistance,
                Gravity          = authoring.Gravity,
                StaticFriction   = authoring.StaticFriction,
                KineticFriction  = authoring.KineticFriction,
            });
            
            // Maybe needed to query collisions, investigate
            AddComponent<PhysicsVelocity>(entity);
        }
    }
}

// Player-unique data
public struct PlayerData : IComponentData
{
    [GhostField] public float MovementSpeed;
    [GhostField] public float JumpHeight;
}

public readonly partial struct PlayerAspect : IAspect
{
    // Reference to the player entity
    public readonly Entity Self;

    // Raw lookup data data from entity components
    private readonly RefRW<LocalTransform> _transform;
    private readonly RefRW<PlayerData> _data;
    private readonly RefRW<HealthComponent> _health;
    
    // Shorthand names for the component data variables (use these mainly)
    public LocalTransform Transform => _transform.ValueRW;
    public float CurrentHealth { get => _health.ValueRO.CurrentHealth; set => _health.ValueRW.CurrentHealth = value; }
    public float MaxHealth     { get => _health.ValueRO.MaxHealth;     set => _health.ValueRW.MaxHealth = value;     }
    public float MovementSpeed { get => _data.ValueRO.MovementSpeed;   set => _data.ValueRW.MovementSpeed = value;   }
    public float JumpStrength  { get => _data.ValueRO.JumpHeight;      set => _data.ValueRW.JumpHeight = value;    }
}
