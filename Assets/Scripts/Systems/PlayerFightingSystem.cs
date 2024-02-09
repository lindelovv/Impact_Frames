using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using System.Collections;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
public partial struct PlayerFightingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<InputComponentData>(); //inputComponent för att komma åt inputscriptets inputs
        state.RequireForUpdate(state.GetEntityQuery(builder));
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        
        state.Dependency = new FightJob {
            DeltaTime = SystemAPI.Time.DeltaTime,
            CollisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld,
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct FightJob : IJobEntity
{
    public float DeltaTime;
    public CollisionWorld CollisionWorld;
    
    // Execute tillhör IJobEntity Interfacet
    public void Execute(PlayerAspect player)
    {
        //Input button logik för att köra punch
        if (player.Input.RequestPunch /* && notInAnimation */)
        {
            Punch(player);
        }

        //Input button logik för att köra kick
        if (player.Input.RequestKick /* && notInAnimation */)
        {
            //Kick(psc);
        }
    }
    
    public void Punch(PlayerAspect player)
    {
        var castPosition = Util.ColliderCast( // Check for blocking hit
            CollisionWorld,
            new PhysicsCollider(), 
            player.Position, 
            player.Position + new float3(10, 0, 0)
        );
        //CheckHit(psc);
        UnityEngine.Debug.Log("Punched!");

        // Set Animation Logic
        // Set VFX Logic
        // Set Sound Logic
    }

    public void Kick(PlayerStateComponent psc)
    {
        if (CanPlayerAttack(psc))
        {
            CheckHit(psc);
            UnityEngine.Debug.Log("Kikecd!");

            // Set Animation Logic
            // Set VFX Logic
            // Set Sound Logic
        }
    }

    private void CheckHit(PlayerStateComponent psc)
    {
        // Har jag träffat en fiendess hurtbox
        //deal damage
        // räkna hur många gånger jag träffa (3)
        //deal more damga
        // kalkylera på varje button klick
    }
    
    private bool CanPlayerAttack(PlayerStateComponent psc)
    {
        return psc.isGrounded && !psc.IsAnimationLocked;
    }
}
