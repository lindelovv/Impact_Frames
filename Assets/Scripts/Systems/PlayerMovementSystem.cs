using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

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
        foreach (var player
                 in SystemAPI.Query<PlayerAspect>()
                )
        {
            // Increase gravity if falling
            {
                if (!player.State.isGrounded && player.Velocity.y <= 1.0f)
                {
                    player.GravityFactor = 5;
                }
                else
                {
                    player.GravityFactor = 1;
                }
            }

            // Calculate & Add Horizontal Movement
            {
                if (player.Input.RequestedMovement.x == 0) // If not moving, change velocity towards 0
                {
                    player.Velocity = new float3(Util.moveTowards(
                        player.Velocity.x,
                        player.Position.x,
                        player.Damping * SystemAPI.Time.DeltaTime
                    ), player.Velocity.y, 0);
                }
                else
                {
                    player.Velocity = new float3(Util.moveTowards( // Else towards max speed
                        player.Velocity.x,
                        player.Input.RequestedMovement.x * player.MaxSpeed,
                        player.Acceleration * SystemAPI.Time.DeltaTime
                    ), player.Velocity.y, 0);
                    
                    // @TODO: set this to not try to increase velocity into walls
                    var castPosition = Util.ColliderCast( // Check for blocking hit
                        SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld,
                        player.Collider, 
                        player.Position, 
                        player.Position + (player.Velocity * SystemAPI.Time.DeltaTime)
                    );
                }
            }

            // Rotation
            {
                player.Rotation = quaternion.EulerXYZ(
                    0f,
                    (player.State.isFacingRight ? 90f : -89.8f),
                    0f
                );
            }

            // Calculate & Add Jump / Vertical Movement
            {
                player.Velocity += new float3(
                    0, 
                    (player is { Input: { RequestJump: true }, State: { isGrounded: true } }
                        ? player.JumpHeight * SystemAPI.Time.DeltaTime 
                        : 0.0f),
                    0
                );
            }
            //Debug.DrawLine(player.Position, player.Position + (player.Velocity / 2), Color.cyan, 1);
        }
    }
}
