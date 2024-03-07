using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup)), UpdateAfter(typeof(PerformActionSystem))]
public partial struct DamageSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<HealthComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (
            var (health, playerState, damage, id, data, entity)
            in SystemAPI.Query<RefRW<HealthComponent>, RefRW<PlayerStateComponent>, TakeDamage, PlayerId, PlayerData>()
                .WithEntityAccess()
                .WithAll<TakeDamage>()
        ) {
            health.ValueRW.Current -= damage.Amount;
            Debug.Log($"{state.EntityManager.GetName(entity)} Health: {health.ValueRO.Current}");
            

            //playerState.ValueRW.IsHit = true;
            cmdBuffer.RemoveComponent<TakeDamage>(entity);
            
            cmdBuffer.AddComponent(entity, new Action {
                Name = ActionName.HitStun,
                Repeating = false,
                State = ActionState.Startup,
                StartTime = 0,
                ActiveTime = .2f,
                RecoverTime = 0,
            });
            cmdBuffer.AddComponent<DoAction>(entity);

            if (id.Value != 0)
            {
                // Update UI
                UIManager.instance.UpdateHealth(health.ValueRW.Current, id.Value);
                if (health.ValueRO.Current <= 0)
                {
                    UIManager.instance.DecreaseLife(id.Value);
                }
            }
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}

[GhostComponent(
    PrefabType=GhostPrefabType.AllPredicted,
    OwnerSendType = SendToOwnerType.SendToNonOwner
)]
public struct TakeDamage : IComponentData
{
    [GhostField] public float Amount;
}
