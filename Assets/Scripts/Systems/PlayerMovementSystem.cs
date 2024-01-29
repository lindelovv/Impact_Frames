using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using VertexFragment;

[BurstCompile]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct PlayerMovementSystem : ISystem  // Detta hette tidigare MoveSystem, menades det Moves, som i alla saker en player kan g�ra? Isf kan vi flytta tillbaka fightingsystem hit
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<PlayerData>()
            .WithAll<InputComponentData>()  //inputComponent f�r att komma �t inputscriptets inputs
            .WithAll<LocalTransform>(); // f�r att kunna r�ra saker, transform
        state.RequireForUpdate(state.GetEntityQuery(builder));
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {}

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //Movejob har valt deltatime som variablar vi vill ha där och Job är för multithreading
        state.Dependency = new MovementJob { 
            DeltaTime = SystemAPI.Time.DeltaTime,
            CollisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct MovementJob : IJobEntity
{
    public float DeltaTime;
    public CollisionWorld CollisionWorld;
    
    public void Execute( // Execute tillhör IJobEntity Interfacet
        in InputComponentData input,
        in PlayerData playerData,
        in PlayerStateComponent state,
        ref Entity entity,
        ref VelocityComponent velocity,
        ref LocalTransform transform,
        ref PhysicsCollider collider
    ) {
        // Calculate Gravity
        {
            velocity.CurrentVelocity.y += (playerData.Gravity * (state.isGrounded ? 0.0f : 1.0f));
        }
        
        // Calculate Horizontal Movement
        {
            velocity.CurrentVelocity.x += (input.HorizontalMoveValue * playerData.MovementSpeed);
        }

        // Calculate Jump / Vertical Movement
        {
            velocity.CurrentVelocity.y += (input.Jump && state.isGrounded ? 0.0f : 1.0f) * playerData.JumpStrength;
        }

        // Apply all movement to player
        {
            var collisions = PhysicsUtils.ColliderCastAll(
                collider,
                transform.Position,
                transform.Position + velocity.CurrentVelocity,
                ref CollisionWorld,
                entity
            );
            if (collisions.Length != 0)
            {
                RigidTransform rigidTransform = new RigidTransform {
                    pos = transform.Position + velocity.CurrentVelocity,
                };
                if (PhysicsUtils.ColliderDistance(
                        out DistanceHit penetration,
                        collider,
                        1.0f,
                        rigidTransform,
                        ref CollisionWorld,
                        entity,
                        PhysicsCollisionFilters.DynamicWithPhysical,
                        null)
                ) {
                    if (penetration.Distance < 0.0f)
                    {
                        velocity.CurrentVelocity += (penetration.SurfaceNormal * -penetration.Distance);
                        if (PhysicsUtils.ColliderCast(
                                out ColliderCastHit adjustedHit,
                                collider,
                                transform.Position,
                                transform.Position + velocity.CurrentVelocity,
                                ref CollisionWorld,
                                entity,
                                PhysicsCollisionFilters.DynamicWithPhysical)
                        )
                        {
                            velocity.CurrentVelocity *= adjustedHit.Fraction;
                        }
                    }
                }
            }
            transform.Position += velocity.CurrentVelocity * DeltaTime;
        }
    }
}
