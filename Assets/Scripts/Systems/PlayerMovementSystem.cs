using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;

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
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {}

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new MovementJob { DeltaTime = SystemAPI.Time.DeltaTime } //Movejob har valt deltatime som variablar vi vill ha d�r och Job �r f�r multithreading
            .ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct MovementJob : IJobEntity
{
    public float DeltaTime;
    public void Execute( // Execute tillh�r IJobEntity Interfacet
        in InputComponentData input,
        in PlayerData playerData,
        in PlayerStateComponent state,
        ref LocalTransform transform
        //ref PhysicsVelocity physicsVelocity,
        //ref PhysicsMass physicsMass
    ) {
        //// Gravity
        //{
        //    if (!state.isGrounded) {
        //        transform.Position.y += playerData.Gravity;
        //    }
        //}
        
        // Movement
        {
            transform.Position.x = (input.MoveValue.x * playerData.MovementSpeed) * DeltaTime;
        }

        //// Jump
        //{
        //    if (input.Jump && state.isGrounded) {
        //        physicsVelocity.ApplyLinearImpulse(physicsMass, new float3(0, playerData.JumpStrength, 0));
        //        //move += new float2(0, input.JumpValue) * playerData.JumpStrength; // replaced with above when state check working
        //    }
        //}
    }
}
