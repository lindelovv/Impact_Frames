using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

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
            .WithAll<InputData>() //inputComponent för att komma åt inputscriptets inputs
            .WithAll<LocalTransform>(); // för att kunna röra saker, transform

        // Components in RequireForUpdate are all types we need to run system (the query above and the physics world)
        state.RequireForUpdate(state.GetEntityQuery(builder));
        state.RequireForUpdate<PhysicsWorldSingleton>();
        
        state.RequireForUpdate<NetworkTime>();
    }

    [BurstCompile]
    
    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (
            var player
            in SystemAPI.Query<PlayerAspect>()
        ) {
            // Increase gravity if falling
            {
                if (player.Data.OverrideGravity)
                {
                    player.GravityFactor = player.Data.CustomGravity;
                }
                else
                {
                    player.GravityFactor = player is { IsGrounded: false, Velocity: { y: <= 2.0f } }
                        ? player.FallGravity
                        : 1;
                }

                var velocityY = player.Velocity.y;
                if (player.IsJumping && velocityY > 2f)
                {
                    velocityY *= player.JumpDecay;
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
                else if (player is { IsAnimLocked: false, IsBlocking: false })
                {
                    player.Velocity = new float3(Util.moveTowards( // Else towards max speed
                        player.Velocity.x,
                        player.Input.RequestedMovement.x * player.MaxSpeed,
                        player.Acceleration * SystemAPI.Time.DeltaTime
                    ), player.Velocity.y, 0);
                    player.IsFacingRight = (player.Input.RequestedMovement.x > 0.0f);
                }
                else
                {
                    player.IsFacingRight = (player.Input.RequestedMovement.x > 0.0f);
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

            // Apply impact
            {
                if (state.EntityManager.HasComponent<ApplyImpact>(player.Self) 
                ) {
                    player.Velocity += new float3(state.EntityManager.GetComponentData<ApplyImpact>(player.Self).Amount, 0);
                    cmdBuffer.RemoveComponent<ApplyImpact>(player.Self);
                }
            }
            //Debug.DrawLine(player.Position, player.Position + (player.Velocity / 2), Color.cyan, 1);
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}
