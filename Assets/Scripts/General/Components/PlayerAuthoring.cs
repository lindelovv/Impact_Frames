using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

public class PlayerAuthoring : MonoBehaviour
{
    //_______________________________________________________________
    [Header("Movement")] 
    
    [Tooltip("[float] How fast the player can move.")]
    public float Speed;
    
    [Tooltip("[float] Maximum movespeed.")]
    public float MaxSpeed;
    
    [Tooltip("[float] How high the player can jump.")]
    public float JumpHeight;
    [Tooltip("[float] How fast jump decays.")]
    public float JumpDecay;
    
    [Tooltip("[float] How fast dash.")]
    public float DashSpeed;

    // Philips variabler

    [Tooltip("[float] Delta time sience last grounded set to 0.25f")]
    public float CayoteTime = 0.25f;

    //_______________________________________________________________
    [Header("Physics")] 
    
    [Tooltip("[float] Movement resistance.")]
    public float Damping;
    
    [Tooltip("[float3] Override starting velocity. Normally should be {0,0,0}.")]
    public float3 StartingVelocity;

    public float FallGravity;
    public bool OverrideGravity;
    public float Gravity;
    
    //_______________________________________________________________
    [Header("Attacks")] 
    
    [Tooltip("[float2] Strength and direction of pushback.")]
    public float2 Pushback;
    
    [Tooltip("[float] Max time between attacks for it to count towards combo counter.")]
    public float MaxComboDelay;
    
    public float BlockCooldown;
    
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
    public class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new Player {
                MovementSpeed = authoring.Speed,
                MaxSpeed      = authoring.MaxSpeed,
                JumpHeight    = authoring.JumpHeight,
                JumpDecay     = authoring.JumpDecay,
                DashSpeed     = authoring.DashSpeed,
                Pushback      = authoring.Pushback,
                MaxComboDelay = authoring.MaxComboDelay,
                OverrideGravity = authoring.OverrideGravity,
                CustomGravity = authoring.Gravity,
                FallGravity   = authoring.FallGravity,
                CayoteTime    = authoring.CayoteTime,
                BlockCooldown = authoring.BlockCooldown,
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
            });
            
            AddComponent(entity, new Action {
                State = ActionState.None,
                DoAction = false,
            });
            AddComponent(entity, new PlayerId {
                Value = 1,
            });
            AddComponent(entity, new ApplyImpact {
                Amount = 0f,
            });
            
            if (!authoring.IsDummy)
            {
                //// Needed to query collisions
                AddComponent(entity, new PhysicsVelocity {
                    Linear = authoring.StartingVelocity,
                });
                AddComponent(entity, new PhysicsDamping {
                    Linear = authoring.Damping,
                    Angular = 9999999,
                });
                AddComponent(entity, new PhysicsMass {
                    Transform = RigidTransform.identity,
                    InverseMass = 1,
                    InverseInertia = 0,
                    AngularExpansionFactor = 0,
                });
                AddComponent<PhysicsGravityFactor>(entity);
            }
        }
    }
}

// Player-unique data
[PredictAll]
public struct Player : IComponentData
{
    [GhostField(Quantization = 0)] public float MovementSpeed;
    [GhostField(Quantization = 0)] public float MaxSpeed;
    [GhostField(Quantization = 0)] public float JumpHeight;
    [GhostField(Quantization = 0)] public float JumpDecay;
    [GhostField(Quantization = 0)] public float DashSpeed;
    [GhostField(Quantization = 0)] public float2 Pushback;
    [GhostField(Quantization = 0)] public float MaxComboDelay;
    [GhostField(Quantization = 0)] public float FallGravity;
    [GhostField(Quantization = 0)] public bool OverrideGravity;
    [GhostField(Quantization = 0)] public float CustomGravity;
    [GhostField(Quantization = 0)] public float CayoteTime;
    [GhostField(Quantization = 0)] public float CayoteTimer;
    [GhostField(Quantization = 0)] public float BlockCooldown;
    [GhostField(Quantization = 0)] public float BlockTimer;
    [GhostField] public bool IsDummy;
    // Jump time
    [GhostField(Quantization = 0)] public float jStartTime;
    [GhostField(Quantization = 0)] public float jActiveTime;
    [GhostField(Quantization = 0)] public float jRecoverTime;
    // Dash time
    [GhostField(Quantization = 0)] public float dStartTime;
    [GhostField(Quantization = 0)] public float dActiveTime;
    [GhostField(Quantization = 0)] public float dRecoverTime;
    // Punch time
    [GhostField(Quantization = 0)] public float pStartTime;
    [GhostField(Quantization = 0)] public float pActiveTime;
    [GhostField(Quantization = 0)] public float pRecoverTime;
    // Heavy Punch
    [GhostField(Quantization = 0)] public float hpStartTime;
    [GhostField(Quantization = 0)] public float hpActiveTime;
    [GhostField(Quantization = 0)] public float hpRecoverTime;
    // Jump time
    [GhostField(Quantization = 0)] public float kStartTime;
    [GhostField(Quantization = 0)] public float kActiveTime;
    [GhostField(Quantization = 0)] public float kRecoverTime;
    // Jump time
    [GhostField(Quantization = 0)] public float hkStartTime;
    [GhostField(Quantization = 0)] public float hkActiveTime;
    [GhostField(Quantization = 0)] public float hkRecoverTime;
}

public struct PlayerId : IComponentData
{
    public Int16 Value;
}

//public readonly partial struct NetPlayerAspect : IAspect
//{
//    public readonly Entity Self;
//    private readonly RefRW<PlayerId> _id;
//    public Int16 Id {
//        get => _id.ValueRO.Value;  
//        set => _id.ValueRW.Value = value;
//    }
//}

public readonly partial struct PlayerAspect : IAspect
{
    // Reference to the player entity
    public readonly Entity Self;

    // Raw lookup data data from entity components
    private readonly RefRW<Player> _data;
    private readonly RefRW<Input> _input;
    private readonly RefRW<PlayerState> _state;
    private readonly RefRW<PhysicsVelocity> _velocity;
    private readonly RefRW<PhysicsDamping> _damping;
    private readonly RefRW<PhysicsGravityFactor> _gravity;
    private readonly RefRW<LocalTransform> _transform;
    
    // Shorthand names for the component data variables (use these for access)
    public Player Data     { get => _data.ValueRO;                set => _data.ValueRW = value;                }
    public Input Input     { get => _input.ValueRO;               set => _input.ValueRW = value;               }
    
    // Transform
    public float3 Position     { get => _transform.ValueRO.Position;  set => _transform.ValueRW.Position = value;  }
    public quaternion Rotation { get => _transform.ValueRO.Rotation;  set => _transform.ValueRW.Rotation = value;  }
    
    // Movement
    public float3 Velocity     { get => _velocity.ValueRO.Linear;     set => _velocity.ValueRW.Linear = value;     }
    public float Damping       { get => _damping.ValueRO.Linear;      set => _damping.ValueRW.Linear = value;      }
    public float GravityFactor { get => _gravity.ValueRO.Value;       set => _gravity.ValueRW.Value = value;       }
    public float FallGravity   { get => _data.ValueRO.FallGravity;    set => _data.ValueRW.FallGravity = value;    }
    public float Acceleration  { get => _data.ValueRO.MovementSpeed;  set => _data.ValueRW.MovementSpeed = value;  }
    public float MaxSpeed      { get => _data.ValueRO.MaxSpeed;       set => _data.ValueRW.MaxSpeed = value;       }
    public float JumpHeight    { get => _data.ValueRO.JumpHeight;     set => _data.ValueRW.JumpHeight = value;     }
    public float JumpDecay     { get => _data.ValueRO.JumpDecay;      set => _data.ValueRW.JumpDecay = value;      }
    public float DashSpeed     { get => _data.ValueRO.DashSpeed;      set => _data.ValueRW.DashSpeed = value;      }
    public float CayoteTime    { get => _data.ValueRO.CayoteTime;     set => _data.ValueRW.CayoteTime = value;     }
    public float CayoteTimer   { get => _data.ValueRO.CayoteTimer;    set => _data.ValueRW.CayoteTimer = value;    }
    
    // State variables
    public bool IsMoving       { get => _state.ValueRO.IsMoving;      set => _state.ValueRW.IsMoving = value;      }          
    public bool IsGrounded     { get => _state.ValueRO.IsGrounded;    set => _state.ValueRW.IsGrounded = value;    }
    public bool IsFacingRight  { get => _state.ValueRO.IsFacingRight; set => _state.ValueRW.IsFacingRight = value; }
    public bool IsFalling      { get => _state.ValueRO.IsFalling;     set => _state.ValueRW.IsFalling = value;     }
    public bool IsFallingHigh  { get => _state.ValueRO.IsFallingHigh; set => _state.ValueRW.IsFallingHigh = value; }
    public bool IsJumping      { get => _state.ValueRO.IsJumping;     set => _state.ValueRW.IsJumping = value;     }
    public bool IsPunching     { get => _state.ValueRO.IsPunching;    set => _state.ValueRW.IsPunching = value;    }
    public bool IsKicking      { get => _state.ValueRO.IsKicking;     set => _state.ValueRW.IsKicking = value;     }
    public bool IsBlocking     { get => _state.ValueRO.IsBlocking;    set => _state.ValueRW.IsBlocking = value;    }
    public bool IsParrying     { get => _state.ValueRO.IsParrying;    set => _state.ValueRW.IsParrying = value;    }
    public bool IsDashing      { get => _state.ValueRO.IsDashing;     set => _state.ValueRW.IsDashing = value;     }
    public bool IsOnBeat       { get => _state.ValueRO.IsOnBeat;      set => _state.ValueRW.IsOnBeat = value;      }
    public bool IsAnimLocked   { get => _state.ValueRO.IsAnimLocked;  set => _state.ValueRW.IsAnimLocked = value;  }
    public bool IsHit          { get => _state.ValueRO.IsHit;         set => _state.ValueRW.IsHit = value;         }
    public bool HasHit         { get => _state.ValueRO.HasHit;        set => _state.ValueRW.HasHit = value;        }
    
    // Fighting/Combo stats
    public int HitCounter      { get => _state.ValueRO.HitCounter;    set => _state.ValueRW.HitCounter = value;    }
    public float HitTime       { get => _state.ValueRO.HitTime;       set => _state.ValueRW.HitTime = value;       }
    public float MaxComboDelay { get => _data.ValueRO.MaxComboDelay;  set => _data.ValueRW.MaxComboDelay = value;  }
    public float2 Pushback     { get => _data.ValueRO.Pushback;       set => _data.ValueRW.Pushback = value;       }
    public float BlockCooldown { get => _data.ValueRO.BlockCooldown;  set => _data.ValueRW.BlockCooldown = value;  }
    public float BlockTimer    { get => _data.ValueRO.BlockTimer;     set => _data.ValueRW.BlockTimer = value;     }
    
    // Misc
    public bool IsDummy        { get => _data.ValueRO.IsDummy;        set => _data.ValueRW.IsDummy = value;        }
    public int Random          { get => _state.ValueRO.Random;        set => _state.ValueRW.Random = value;        }
}
