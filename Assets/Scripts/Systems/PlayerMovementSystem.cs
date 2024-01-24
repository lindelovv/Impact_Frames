using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct PlayerMovementSystem : ISystem  // Detta hette tidigare MoveSystem, menades det Moves, som i alla saker en player kan göra? Isf kan vi flytta tillbaka fightingsystem hit
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<PlayerComponent>()
            .WithAll<InputComponentData>()  //inputComponent för att komma åt inputscriptets inputs
            .WithAll<LocalTransform>(); // för att kunna röra saker, transform
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {}

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var moveJob = new MoveJob { DeltaTime = SystemAPI.Time.DeltaTime };  //Movejob har valt deltatime som variablar vi vill ha där och Job är för multithreading
        state.Dependency = moveJob.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct MoveJob : IJobEntity
{
    public float DeltaTime;
    public void Execute(in InputComponentData input, in PlayerComponent playerData, in PlayerStateComponent state,
                        ref LocalTransform transform, ref PhysicsVelocity physicsVelocity, ref PhysicsMass physicsMass)  // Execute tillhör IJobEntity Interfacet
    {
        // Movement
        var move = new float2(input.MoveValue.x, 0) * playerData.MovementSpeed;
        
        // Jump
        //if (state.isGrounded)
        //{
        //    physicsVelocity.ApplyLinearImpulse(physicsMass, new float3(0, input.JumpValue, 0) * playerData.JumpStrength);
        //}
        move += new float2(0, input.JumpValue) * playerData.JumpStrength; // replaced with above when state check working
        
        // Move
        transform.Position += new float3(move.x, move.y, 0) * DeltaTime;
    }
}
