using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.Rendering;

//[UpdateInGroup(typeof(PredictedSimulationSystemGroup)), UpdateAfter(typeof(ActionSystem))]
public partial struct DamageSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Health>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (
            var (health, playerState, damage, id, data, entity)
            in SystemAPI.Query<HealthAspect, RefRW<PlayerState>, TakeDamage, PlayerId, Player>()
                .WithEntityAccess()
                .WithAll<TakeDamage>()
        ) {
            health.Current -= damage.Amount;
            Debug.Log($"{state.EntityManager.GetName(entity)} Health: {health.Current}/{health.Max}");
            
            cmdBuffer.RemoveComponent<TakeDamage>(entity);
            
            cmdBuffer.SetComponent(entity, new Action {
                Name = ActionName.HitStun,
                State = ActionState.Startup,
                StartTime = 0,
                ActiveTime = 0,
                RecoverTime = .5f,
            });

            if (id.Value != 0)
            {
                // Update UI
                UIManager.instance.UpdateHealth(health.Current, id.Value);
                if (health.Current <= 0)
                {
                    cmdBuffer.AddComponent<Respawn>(entity);
                    health.Current = health.Max;
                    UIManager.instance.UpdateHealth(health.Current, id.Value);
                    UIManager.instance.DecreaseLife(id.Value);
                }
            }
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}
