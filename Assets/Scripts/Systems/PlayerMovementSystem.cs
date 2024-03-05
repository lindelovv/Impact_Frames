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
        
        state.RequireForUpdate<NetworkTime>();
    }

    [BurstCompile]
    
    public void OnUpdate(ref SystemState state)
    {



       


        if (SystemAPI.GetSingleton<NetworkTime>().IsFinalPredictionTick)
        {
            var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
            foreach (
                var player
                in SystemAPI.Query<PlayerAspect>()
            ) {
                // Increase gravity if falling
                {
                    player.GravityFactor = player is { IsGrounded: false, Velocity: { y: <= 1.0f } }
                        ? 5
                        : 1;
                }

                // Calculate & Add Horizontal Movement
                {
                   
                    
                    if (player.IsDashing)
                    {
                        player.Velocity = new float3(player.IsFacingRight ? 25f : -25f, 3f, 0);
                    }

                    if (player.Input.RequestedMovement.x == 0) // If not moving, change velocity towards 0
                    {
                        player.Velocity = new float3(Util.moveTowards(
                            player.Velocity.x,
                            player.Position.x,
                            player.Damping * SystemAPI.Time.DeltaTime
                        ), player.Velocity.y, 0);
                    }
                    else if (!player.IsBlocking)
                    {
                        player.Velocity = new float3(Util.moveTowards( // Else towards max speed
                            player.Velocity.x,
                            player.Input.RequestedMovement.x * player.MaxSpeed,
                            player.Acceleration * SystemAPI.Time.DeltaTime
                            //player.Acceleration *= Mathf.Pow(1f- player.Damping, Time.deltaTime *10f
                        ), player.Velocity.y, 0);
                    }
                }

                // Rotation
                {
                    player.Rotation = quaternion.EulerXYZ(
                        0f,
                        (player.IsFacingRight ? 90f : -89.8f),
                        0f
                    );
                }

                // Calculate & Add Jump / Vertical Movement
                {
                    player.JumpPressRemember -= Time.deltaTime;
                    Debug.Log(player.JumpPressRemember);


                    if (player.Input.RequestJump)
                    {
                        player.JumpPressRemember = player.JumpPressTimer; // Timer set to 0.2


                        player.Velocity += new float3(
                            0,
                            (player is { IsGrounded: true } //or { IsOnBeat: true }
                                ? player is { IsFalling: true }
                                    ? -player.Velocity.y + player.JumpHeight * SystemAPI.Time.DeltaTime
                                    : player.JumpHeight * SystemAPI.Time.DeltaTime
                                : 0.0f),
                            0
                        );
                    }
                    // Check if you have Coyote or Groundbuffer
                    if (player.JumpPressRemember > 0 && player.fGroundedRemember > 0) {
                        Debug.Log("HOPPPPPAAAR MED COYOTE ELLER GROUNDBUFFER");
                        player.JumpPressRemember = 0;
                        player.JumpPressTimer = 0;

                        // Adding a new Jumpforce for that state
                        player.Velocity += new float3(  
                           0,
                           (player is { IsGrounded: true } //or { IsOnBeat: true }
                               ? player is { IsFalling: true }
                                   ? -player.Velocity.y + player.JumpHeight * SystemAPI.Time.DeltaTime
                                   : player.JumpHeight * SystemAPI.Time.DeltaTime
                               : 0.0f),
                           0
                        );

                    }
                }
                //Debug.DrawLine(player.Position, player.Position + (player.Velocity / 2), Color.cyan, 1);

                // Apply impact
                {
                    if (state.EntityManager.HasComponent<ApplyImpact>(player.Self) 
                    ) {
                        player.Velocity += new float3(state.EntityManager.GetComponentData<ApplyImpact>(player.Self).Amount, 0);
                        cmdBuffer.RemoveComponent<ApplyImpact>(player.Self);
                    }
                }
            }

            cmdBuffer.Playback(state.EntityManager);
            cmdBuffer.Dispose();
        }
    }
}
