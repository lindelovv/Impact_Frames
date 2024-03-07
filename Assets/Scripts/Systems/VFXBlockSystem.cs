using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.VFX;

[GhostComponent(
    PrefabType=GhostPrefabType.AllPredicted,
    OwnerSendType = SendToOwnerType.SendToNonOwner
)]
public struct SyncBlockVFX : IComponentData {}

//______________________________________________________________________________________________________________________
public partial struct VFXBlockInitSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamInGame>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var cmdBuffer = new EntityCommandBuffer(Allocator.TempJob);

        foreach (
            var (vfxGameObject, entity)
            in SystemAPI.Query<VFXBlockGameObject>()
                .WithNone<VFXBlockReferenceData>()
                .WithEntityAccess()
        ) {
            var gameObject = Object.Instantiate(vfxGameObject.Prefab);
            var animationReference = new VFXBlockReferenceData {
                VFX = gameObject.GetComponent<VisualEffect>(),
            };
            cmdBuffer.AddComponent(entity, animationReference);
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}

//______________________________________________________________________________________________________________________
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct VFXBlockSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<VFXBlockReferenceData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (
            var (reference, playerState, transform)
            in SystemAPI.Query<VFXBlockReferenceData, PlayerStateComponent, LocalTransform>()
        ) {
            reference.VFX.transform.position = new float3(
                transform.Position.x,
                transform.Position.y + 1,
                transform.Position.z
            );
            
            if (playerState.IsBlocking /*&& !reference.VFX.HasAnySystemAwake()*/)
            {
                reference.VFX.enabled = true;
            }
            else // if(reference.VFX.isActiveAndEnabled)
            {
                reference.VFX.enabled = false;
            }
            
        }
    }
}