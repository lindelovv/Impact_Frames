using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using BoxCollider = Unity.Physics.BoxCollider;
using Collider = Unity.Physics.Collider;
using RaycastHit = Unity.Physics.RaycastHit;

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
    public void OnUpdate(ref SystemState state)
    {
        
        //state.Dependency = new FightJob {
        //    DeltaTime = SystemAPI.Time.DeltaTime,
        //    CollisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld,
        //}.ScheduleParallel(state.Dependency);
        foreach (var player in SystemAPI.Query<PlayerAspect>())
        {
            //Input button logik för att köra punch
            if (player.Input.RequestPunch /* && notInAnimation */)
            {
                Punch(player);
            }

            //Input button logik för att köra kick
            if (player.Input.RequestKick /* && notInAnimation */)
            {
                Kick(player);
            }
        }
    }
    
    public void Punch(PlayerAspect player)
    {
        var forward = player.State.isFacingRight ? 1 : -1;
        
        RaycastHit hit = new RaycastHit();
        bool hasHit = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld.CastRay(new RaycastInput {
            Filter = CollisionFilter.Default,
            Start = player.Position + (forward * new float3(0.9f, 0, 0)),
            End = player.Position + (forward * new float3(1, 0, 0)),
        }, out hit);
        
        Debug.DrawLine(player.Position + (forward * new float3(0.9f, 0, 0)), player.Position + (forward * new float3(1, 0, 0)), Color.magenta, 1);
        Debug.DrawLine(player.Position + (forward * new float3(0.95f, 0.05f, 0)), player.Position + (forward * new float3(0.95f, -0.05f, 0)), Color.magenta, 1);
        
        if (hasHit)
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Debug.Log($"hit { entityManager.GetName(hit.Entity) }");
        }

        // Set Animation Logic
        // Set VFX Logic
        // Set Sound Logic
    }
    
    public void Kick(PlayerAspect player)
    {
        UnityEngine.Debug.Log("Kick");

        // Set Animation Logic
        // Set VFX Logic
        // Set Sound Logic
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
        UnityEngine.Debug.Log("punch");
        var filter = new CollisionFilter {
            BelongsTo = ~0u,
            CollidesWith = ~0u,
            GroupIndex = 0,
        };
        BoxGeometry boxGeometry = new BoxGeometry {
            Center = float3.zero,
            Size = 5,
        };
        BlobAssetReference<Collider> boxCollider = BoxCollider.Create(boxGeometry, filter);
        unsafe
        {
            ColliderCastInput castInput = new ColliderCastInput {
                Collider = (Collider*)boxCollider.GetUnsafePtr(),
                Orientation = quaternion.identity,
                Start = player.Position,
                End = player.Position + new float3(10, 0, 0),
            };
            ColliderCastHit castHit = new ColliderCastHit {};
            bool bHit = CollisionWorld.CastCollider(castInput, out castHit);
            if (bHit) { UnityEngine.Debug.Log("hit"); }
        }
        
        //var castPosition = Util.ColliderCast( // Check for blocking hit
        //    CollisionWorld,
        //    new PhysicsCollider(), 
        //    player.Position, 
        //    player.Position + new float3(10, 0, 0)
        //);
        //CheckHit(psc);
        //UnityEngine.Debug.Log("Punched!");

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
