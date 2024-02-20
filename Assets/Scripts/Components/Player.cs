using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

public class Player : MonoBehaviour
{
    //_______________________________________________________________
    [Header("Health")] 
    
    [Tooltip("[float] Remaining health points.")]
    public float CurrentHealth;
    
    [Tooltip("[float] The maximum health possible.")]
    public float MaxHealth;
    
    //_______________________________________________________________
    [Header("Movement")] 
    
    [Tooltip("[float] How fast the player can move.")]
    public float Speed;
    
    [Tooltip("[float] Maximum movespeed.")]
    public float MaxSpeed;
    
    [Tooltip("[float] How high the player can jump.")]
    public float JumpHeight;
    
    //_______________________________________________________________
    [Header("Physics")] 
    
    [Tooltip("[float] Movement resistance.")]
    public float Damping;
    
    [Tooltip("[float3] Override starting velocity. Normally should be {0,0,0}.")]
    public float3 StartingVelocity;
    
    //_______________________________________________________________
    [Header("Attacks")] 
    
    [Tooltip("[float2] Strength and direction of punch pushback.")]
    public float2 PunchPushback;
    
    
    //_______________________________________________________________
    public class Baker : Baker<Player>
    {
        public override void Bake(Player authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new PlayerData {
                MovementSpeed = authoring.Speed,
                MaxSpeed      = authoring.MaxSpeed,
                JumpHeight    = authoring.JumpHeight,
                PunchPushback = authoring.PunchPushback,
            });
            
            // Health component is it's own component in case of reuse,
            // but since we always want one on the player it is added here in code
            // from the values from this authoring component (see [Header("Health")] above).
            AddComponent(entity, new HealthComponent {
                Current = authoring.CurrentHealth,
                Max     = authoring.MaxHealth,
            });
            
            // Needed to query collisions
            AddComponent(entity, new PhysicsVelocity {
                Linear = authoring.StartingVelocity,
            });
            AddComponent(entity, new PhysicsDamping {
                Linear  = authoring.Damping,
                Angular = 9999999,
            });
            AddComponent(entity, new PhysicsMass {
                Transform              = RigidTransform.identity,
                InverseMass            = 1,
                InverseInertia         = 0,
                AngularExpansionFactor = 0,
            });
            AddComponent<PhysicsGravityFactor>(entity);
            
            // Might switch this out later to add/remove as needed instead
            AddComponent(entity, new ApplyImpact {
                Amount = 0f,
            });
        }
    }
}

// Player-unique data
public struct PlayerData : IComponentData
{
    [GhostField] public float MovementSpeed;
    [GhostField] public float MaxSpeed;
    [GhostField] public float JumpHeight;
    public float2 PunchPushback;
}

public readonly partial struct PlayerAspect : IAspect
{
    // Reference to the player entity
    public readonly Entity Self;

    // Raw lookup data data from entity components
    private readonly RefRW<PlayerData> _data;
    private readonly RefRW<HealthComponent> _health;
    private readonly RefRW<InputComponentData> _input;
    private readonly RefRW<PlayerStateComponent> _state;
    private readonly RefRW<PhysicsCollider> _collider;
    private readonly RefRW<PhysicsVelocity> _velocity;
    private readonly RefRW<PhysicsDamping> _damping;
    private readonly RefRW<PhysicsGravityFactor> _gravityFactor;
    private readonly RefRW<LocalTransform> _transform;
    
    // Shorthand names for the component data variables (use these for access)
        public float3 Position      { get => _transform.ValueRO.Position;  set => _transform.ValueRW.Position = value;  }
    public quaternion Rotation      { get => _transform.ValueRO.Rotation;  set => _transform.ValueRW.Rotation = value;  }
    public PhysicsCollider Collider { get => _collider.ValueRO;            set => _collider.ValueRW = value;            }
    public float3 Velocity          { get => _velocity.ValueRO.Linear;     set => _velocity.ValueRW.Linear = value;     }
    public float Damping            { get => _damping.ValueRO.Linear;      set => _damping.ValueRW.Linear = value;      }
    public float GravityFactor      { get => _gravityFactor.ValueRO.Value; set => _gravityFactor.ValueRW.Value = value; }
    public float Acceleration       { get => _data.ValueRO.MovementSpeed;  set => _data.ValueRW.MovementSpeed = value;  }
    public float MaxSpeed           { get => _data.ValueRO.MaxSpeed;       set => _data.ValueRW.MaxSpeed = value;       }
    public float JumpHeight         { get => _data.ValueRO.JumpHeight;     set => _data.ValueRW.JumpHeight = value;     }
    public float CurrentHealth      { get => _health.ValueRO.Current;      set => _health.ValueRW.Current = value;      }
    public float MaxHealth          { get => _health.ValueRO.Max;          set => _health.ValueRW.Max = value;          }
    public float2 PunchPushback     { get => _data.ValueRO.PunchPushback;  set => _data.ValueRW.PunchPushback = value;  }
    public InputComponentData Input { get => _input.ValueRO;               set => _input.ValueRW = value;               }
    public bool IsMoving            { get => _state.ValueRO.isMoving;      set => _state.ValueRW.isMoving = value;      }          
    public bool IsGrounded          { get => _state.ValueRO.isGrounded;    set => _state.ValueRW.isGrounded = value;    }
    public bool IsFacingRight       { get => _state.ValueRO.isFacingRight; set => _state.ValueRW.isFacingRight = value; }
    public bool IsFalling           { get => _state.ValueRO.isFalling;     set => _state.ValueRW.isFalling = value;     }
    public bool IsJumping           { get => _state.ValueRO.isJumping;     set => _state.ValueRW.isJumping = value;     }
    public bool IsPunching          { get => _state.ValueRO.isPunching;    set => _state.ValueRW.isPunching = value;    }
    public bool IsBlocking          { get => _state.ValueRO.isBlocking;    set => _state.ValueRW.isBlocking = value;    }
    public bool IsParrying          { get => _state.ValueRO.isParrying;    set => _state.ValueRW.isParrying = value;    }
}
