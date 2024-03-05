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

    // Philips variabler

    [Tooltip("[float] Jump Remember")]
    public float JumpPressRemember;

    [Tooltip("[float] Delta time sience last klicked set to 0.2f")]
    public float JumpPressTimer = 0.2f;

    [Tooltip("[float] Ground Remember")]
    public float GroundedRemember;

    [Tooltip("[float] Delta time sience last grounded set to 0.25f")]
    public float GroundedRememberTime = 0.25f;

    //_______________________________________________________________
    [Header("Physics")] 
    
    [Tooltip("[float] Movement resistance.")]
    public float Damping;
    
    [Tooltip("[float3] Override starting velocity. Normally should be {0,0,0}.")]
    public float3 StartingVelocity;

    public bool OverrideGravity;
    public float Gravity;
    
    //_______________________________________________________________
    [Header("Attacks")] 
    
    [Tooltip("[float2] Strength and direction of punch pushback.")]
    public float2 PunchPushback;
    
    [Tooltip("[float] Max time between attacks for it to count towards combo counter.")]
    public float MaxComboDelay;
    
    //_______________________________________________________________
    [Header("Actions\t\t\t\tX = Startup    Y = Active       Z = Recover")] 
    
    public float3 JumpTime;
    public float3 DashTime;
    public float3 PunchTime;
    public float3 HeavyPunchTime;
    public float3 KickTime;
    public float3 HeavyKickTime;
    
    //_______________________________________________________________
    [Header("Misc")] 
    
    [Tooltip("[bool] Disables movement and impulses.")]
    public bool IsDummy;
    
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
                MaxComboDelay = authoring.MaxComboDelay,
                OverrideGravity = authoring.OverrideGravity,
                CustomGravity = authoring.Gravity,
                // Jump time
                jStartTime    = authoring.JumpTime.x,
                jActiveTime   = authoring.JumpTime.y,
                jRecoverTime  = authoring.JumpTime.z,
                // Dash time
                dStartTime    = authoring.DashTime.x,
                dActiveTime   = authoring.DashTime.y,
                dRecoverTime  = authoring.DashTime.z,
                // Punch time
                pStartTime    = authoring.PunchTime.x,
                pActiveTime   = authoring.PunchTime.y,
                pRecoverTime  = authoring.PunchTime.z,
                // Heavy Punch time
                hpStartTime   = authoring.HeavyPunchTime.x,
                hpActiveTime  = authoring.HeavyPunchTime.y,
                hpRecoverTime = authoring.HeavyPunchTime.z,
                // Jump time
                kStartTime    = authoring.KickTime.x,
                kActiveTime   = authoring.KickTime.y,
                kRecoverTime  = authoring.KickTime.z,
                // Jump time
                hkStartTime   = authoring.HeavyKickTime.x,
                hkActiveTime  = authoring.HeavyKickTime.y,
                hkRecoverTime = authoring.HeavyKickTime.z,
                JumpPressRemember = authoring.JumpPressRemember,
                JumpPressTimer = authoring.JumpPressTimer,
                fGroundedRemember = authoring.GroundedRemember,
                fGroundedRememberTime = authoring.GroundedRememberTime,

            });
            
            // Health component is it's own component in case of reuse,
            // but since we always want one on the player it is added here in code
            // from the values from this authoring component (see [Header("Health")] above).
            AddComponent(entity, new HealthComponent {
                Current = authoring.CurrentHealth,
                Max     = authoring.MaxHealth,
            });

            AddComponent(entity, new PlayerId {
                Value = 0,
            });
            if (!authoring.IsDummy)
            {
                //// Needed to query collisions
                AddComponent(entity, new PhysicsVelocity
                {
                    Linear = authoring.StartingVelocity,
                });
                AddComponent(entity, new PhysicsDamping
                {
                    Linear = authoring.Damping,
                    Angular = 9999999,
                });
                AddComponent(entity, new PhysicsMass
                {
                    Transform = RigidTransform.identity,
                    InverseMass = 1,
                    InverseInertia = 0,
                    AngularExpansionFactor = 0,
                });
                AddComponent<PhysicsGravityFactor>(entity);

                // Might switch this out later to add/remove as needed instead
                AddComponent(entity, new ApplyImpact
                {
                    Amount = 0f,
                });
            }
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
    public float MaxComboDelay;
    [GhostField] public bool IsDummy;
    [GhostField] public bool OverrideGravity;
    [GhostField] public float CustomGravity;
    // Jump time
    public float jStartTime;
    public float jActiveTime;
    public float jRecoverTime;
    // Dash time
    public float dStartTime;
    public float dActiveTime;
    public float dRecoverTime;
    // Punch time
    public float pStartTime;
    public float pActiveTime;
    public float pRecoverTime;
    // Heavy Punch
    public float hpStartTime;
    public float hpActiveTime;
    public float hpRecoverTime;
    // Jump time
    public float kStartTime;
    public float kActiveTime;
    public float kRecoverTime;
    // Jump time
    public float hkStartTime;
    public float hkActiveTime;
    public float hkRecoverTime;

    
    public float JumpPressRemember;
    public float JumpPressTimer;
    public float fGroundedRemember;
    public float fGroundedRememberTime;

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
    public PlayerData Data          { get => _data.ValueRO;                set => _data.ValueRW = value;                }
    public bool IsDummy             { get => _data.ValueRO.IsDummy;        set => _data.ValueRW.IsDummy = value;        }
    public float3 Position          { get => _transform.ValueRO.Position;  set => _transform.ValueRW.Position = value;  }
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
    public bool IsMoving            { get => _state.ValueRO.IsMoving;      set => _state.ValueRW.IsMoving = value;      }          
    public bool IsGrounded          { get => _state.ValueRO.IsGrounded;    set => _state.ValueRW.IsGrounded = value;    }
    public bool IsFacingRight       { get => _state.ValueRO.IsFacingRight; set => _state.ValueRW.IsFacingRight = value; }
    public bool IsFalling           { get => _state.ValueRO.IsFalling;     set => _state.ValueRW.IsFalling = value;     }
    public bool IsFallingHigh       { get => _state.ValueRO.IsFallingHigh; set => _state.ValueRW.IsFallingHigh = value; }
    public bool IsJumping           { get => _state.ValueRO.IsJumping;     set => _state.ValueRW.IsJumping = value;     }
    public bool IsPunching          { get => _state.ValueRO.IsPunching;    set => _state.ValueRW.IsPunching = value;    }
    public bool IsKicking           { get => _state.ValueRO.IsKicking;     set => _state.ValueRW.IsKicking = value;     }
    public bool IsBlocking          { get => _state.ValueRO.IsBlocking;    set => _state.ValueRW.IsBlocking = value;    }
    public bool IsParrying          { get => _state.ValueRO.IsParrying;    set => _state.ValueRW.IsParrying = value;    }
    public bool IsDashing           { get => _state.ValueRO.IsDashing;     set => _state.ValueRW.IsDashing = value;     }
    public bool IsOnBeat            { get => _state.ValueRO.IsOnBeat;      set => _state.ValueRW.IsOnBeat = value;      }
    public bool IsAnimLocked        { get => _state.ValueRO.IsAnimLocked;  set => _state.ValueRW.IsAnimLocked = value;  }
    public int HitCounter           { get => _state.ValueRO.HitCounter;    set => _state.ValueRW.HitCounter = value;    }
    public float HitTime            { get => _state.ValueRO.HitTime;       set => _state.ValueRW.HitTime = value;       }
    public float MaxComboDelay      { get => _data.ValueRO.MaxComboDelay;  set => _data.ValueRW.MaxComboDelay = value;  }
   
    public float JumpPressRemember { get => _data.ValueRO.JumpPressRemember; set => _data.ValueRW.JumpPressRemember = value; }

    public float JumpPressTimer { get => _data.ValueRO.JumpPressTimer; set => _data.ValueRW.JumpPressTimer = value; }

    public float fGroundedRemember { get => _data.ValueRO.fGroundedRemember; set => _data.ValueRW.fGroundedRemember = value; }

    public float fGroundedRememberTime { get => _data.ValueRO.fGroundedRememberTime; set => _data.ValueRW.fGroundedRememberTime = value; }

}
