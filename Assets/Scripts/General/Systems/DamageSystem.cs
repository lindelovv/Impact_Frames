using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup)), UpdateAfter(typeof(ActionSystem))]
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
            
            cmdBuffer.RemoveComponent<TakeDamage>(entity);
            
            cmdBuffer.RemoveComponent<Action>(entity);
            cmdBuffer.AddComponent(entity, new Action {
                Name = ActionName.HitStun,
                Repeating = false,
                State = ActionState.Startup,
                StartTime = 0,
                ActiveTime = .2f,
                RecoverTime = 0,
            });
            cmdBuffer.SetComponent(entity, new Action
            {
                Name = ActionName.HitStun,
                State = ActionState.Startup,
                StartTime = 0,
                ActiveTime = 0,
                RecoverTime = .5f,
            });

            if (id.Value != 0)
            {
                // Update UI
                UIManager.instance.UpdateHealth(health.ValueRO.Current, id.Value);
                if (health.ValueRO.Current <= 0)
                {
                    cmdBuffer.AddComponent<Respawn>(entity);
                    health.ValueRW.Current = health.ValueRO.Max;
                    UIManager.instance.UpdateHealth(health.ValueRO.Current, id.Value);
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
