using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.VFX;

public struct SyncVFX : IComponentData {}

//______________________________________________________________________________________________________________________
public partial struct VFXInitSystem : ISystem
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
            in SystemAPI.Query<VFXGameObject>()
                .WithNone<VFXReferenceData>()
                .WithEntityAccess()
        ) {
            var gameObject = Object.Instantiate(vfxGameObject.Prefab);
            var animationReference = new VFXReferenceData {
                VFX = gameObject.GetComponent<VisualEffect>(),
                NameHash = vfxGameObject.NameHash,
            };
            cmdBuffer.AddComponent(entity, animationReference);
        }
        cmdBuffer.Playback(state.EntityManager);
        cmdBuffer.Dispose();
    }
}

//______________________________________________________________________________________________________________________
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct VFXSystem : ISystem
{
    private readonly struct _parameters {
        public static readonly int Block = Animator.StringToHash("Block");
    }

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<VFXReferenceData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (
            var (reference, playerState, transform)
            in SystemAPI.Query<VFXReferenceData, PlayerStateComponent, LocalTransform>()
        ) {
            reference.VFX.transform.position = transform.Position;
            
            if (reference.NameHash == _parameters.Block)
            {
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
}