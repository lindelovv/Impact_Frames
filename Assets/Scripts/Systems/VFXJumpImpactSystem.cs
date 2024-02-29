using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.VFX;

public struct SyncJumpImpactVFX : IComponentData {}

//______________________________________________________________________________________________________________________
public partial struct VFXJumpImpactInitSystem : ISystem
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
            in SystemAPI.Query<VFXJumpImpactGameObject>()
                .WithNone<VFXJumpImpactReferenceData>()
                .WithEntityAccess()
        ) {
            var gameObject = Object.Instantiate(vfxGameObject.Prefab);
            var animationReference = new VFXJumpImpactReferenceData {
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
public partial struct VFXJumpImpactSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<VFXJumpImpactReferenceData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (
            var (reference, playerState, transform)
            in SystemAPI.Query<VFXJumpImpactReferenceData, PlayerStateComponent, LocalTransform>()
        ) {
            reference.VFX.transform.position = new float3(
                transform.Position.x,
                transform.Position.y - 1f,
                transform.Position.z
            );
            if (playerState.IsFallingHigh && playerState.IsGrounded)
            {
                reference.VFX.enabled = true;
            }
            else //if(!reference.VFX.isActiveAndEnabled)
            {
                reference.VFX.enabled = false;
            }
        }
    }
}