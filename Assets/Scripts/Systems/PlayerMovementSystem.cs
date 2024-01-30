using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using VertexFragment;
using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

[BurstCompile]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct PlayerMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Use EntityQueryBuilder to find all entities with the given components.
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<PlayerData>()
            .WithAll<InputComponentData>() //inputComponent för att komma åt inputscriptets inputs
            .WithAll<LocalTransform>(); // för att kunna röra saker, transform

        // Components in RequireForUpdate are all types we need to run system (the query above and the physics world)
        state.RequireForUpdate(state.GetEntityQuery(builder));
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (input, playerData, playerState, velocity, transform, collider, entity)
                 in SystemAPI
                     .Query<InputComponentData, PlayerData, PlayerStateComponent, RefRW<VelocityComponent>,
                         RefRW<LocalTransform>, PhysicsCollider>()
                     .WithEntityAccess()
                )
        {
            // Calculate & Add Gravity
            {
                velocity.ValueRW.CurrentVelocity += (velocity.ValueRW.Gravity * (playerState.isGrounded ? 0.0f : 1.0f)) * SystemAPI.Time.DeltaTime;
            }

            // Calculate & Add Horizontal Movement
            {
                velocity.ValueRW.CurrentVelocity.x += (input.RequstedHorizontalMovement.x * playerData.MovementSpeed) * SystemAPI.Time.DeltaTime;
            }

            // Calculate & Add Jump / Vertical Movement
            {
                velocity.ValueRW.CurrentVelocity.y +=
                    (input.RequestJump && playerState.isGrounded ? playerData.JumpHeight : 0.0f) * SystemAPI.Time.DeltaTime;
            }

            // Apply all movement to player
            {
                UpdateVelocity(collider, transform.ValueRO, entity, ref velocity.ValueRW, ref velocity.ValueRW.CurrentVelocity, SystemAPI.Time.DeltaTime);

                velocity.ValueRW.CurrentVelocity *= math.pow(velocity.ValueRO.AirResistance, SystemAPI.Time.DeltaTime);

                velocity.ValueRW.CurrentVelocity = math.clamp(
                    velocity.ValueRW.CurrentVelocity,
                    -velocity.ValueRO.TerminalVelocity,
                    velocity.ValueRO.TerminalVelocity
                );

                transform.ValueRW.Position += velocity.ValueRW.CurrentVelocity * SystemAPI.Time.DeltaTime;
            }
        }
    }

    private void UpdateVelocity(
        in PhysicsCollider collider,
        in LocalTransform transform,
        in Entity entity,
        ref VelocityComponent velocityComponent,
        ref float3 velocity,
        float deltaTime
    ) {
        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        collisionWorld.UpdateBodyIndexMap();

        if (math.length(velocity) < 0.001f)
        {
            velocity = 0;
        }

        bool bHit = true;
        int loopCount = 0;
        while (bHit && loopCount < 16)
        {
            Debug.Log($"Velocity: {velocity}");
            if (math.length(velocity) * deltaTime < 0.01f)
            {
                velocity = 0;
                return;
            }

            bHit = PhysicsUtils.ColliderCast(
                out ColliderCastHit nearestHit,
                collider,
                transform.Position,
                transform.Position + velocity,
                ref collisionWorld,
                entity
            );

            if (bHit)
            {
                Debug.Log("Hit");
                Debug.DrawLine(transform.Position, nearestHit.Position);
                float distanceToColliderNeg = math.dot(velocity, nearestHit.SurfaceNormal);
                
                if (distanceToColliderNeg < 0)
                {
                    distanceToColliderNeg = -0.001f;
                }

                float allowedDistanceToMove = math.distance(nearestHit.Position, transform.Position) + distanceToColliderNeg;
                
                if (allowedDistanceToMove > math.length(velocity) * deltaTime)
                {
                    return;
                }

                if (allowedDistanceToMove < 0.0f)
                {
                    velocity = math.normalizesafe(velocity) * allowedDistanceToMove;
                }

                float3 normalForce = math.dot(velocity, nearestHit.SurfaceNormal) > 0
                    ? float3.zero
                    : -math.dot(velocity, nearestHit.SurfaceNormal) * nearestHit.SurfaceNormal;

                velocity += normalForce;

                if (math.length(velocity) < math.length(normalForce) * velocityComponent.StaticFriction)
                {
                    velocity = 0;
                }
                else
                {
                    velocity -= math.normalizesafe(velocity) *
                                                math.length(normalForce) * velocityComponent.KineticFriction;
                }
            }
            Debug.Log($"Velocity: {velocity}, Loop: {loopCount}");
            loopCount++;
        }
    }

    private unsafe Entity CollisionSweep(in PhysicsCollider collider, float3 start, float3 end)
    {
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<PhysicsWorldSingleton>();
        EntityQuery singeltonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
        var collisionWorld = singeltonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        
        singeltonQuery.Dispose();
        
        var filter = new CollisionFilter {
            BelongsTo = ~0u,
            CollidesWith = ~0u,
            GroupIndex = 0,
        };

        ColliderCastInput input = new ColliderCastInput
        {
            Collider = collider.ColliderPtr,
            Orientation = quaternion.identity,
            Start = start,
            End = end,
        };
        ColliderCastHit hit = new ColliderCastHit();
        if (collisionWorld.CastCollider(input, out hit))
        {
            return hit.Entity;
        }

        return Entity.Null;
    }
}
