using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using System.Collections;
using Unity.Physics;

[BurstCompile]
public partial struct PlayerFightingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<InputComponentData>(); //inputComponent för att komma åt inputscriptets inputs
            state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var fightJob = new FightJob { DeltaTime = SystemAPI.Time.DeltaTime };
        state.Dependency = fightJob.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct FightJob : IJobEntity
{
    public float DeltaTime;
    public void Execute(in InputComponentData input, in PlayerStateComponent psc)  // Execute tillhör IJobEntity Interfacet
    {

        //Input button logik för att köra punch
        Punch(psc);


        //Input button logik för att köra kick
        Kick(psc);
    }

    
    
    
    public void Punch(PlayerStateComponent psc)
    {
        if (CanPlayerAttack(psc))
        {
            
            CheckHit(psc);
            UnityEngine.Debug.Log("Punched!");

            // Set Animation Logic
            // Set VFX Logic
            // Set Sound Logic
        }
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
