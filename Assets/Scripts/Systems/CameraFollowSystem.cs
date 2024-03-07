using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(TransformSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class UpdateCameraTargetSystem : SystemBase
{
    private Transform _cameraTarget;

    protected override void OnCreate()
    {
        RequireForUpdate<InputData>();
        RequireForUpdate<NetworkId>();
    }

    protected override void OnStartRunning()
    {
        _cameraTarget = GameObject.FindWithTag("CameraTarget").transform;
        base.OnStartRunning();
    }

    protected override void OnUpdate()
    {
        foreach (
            var transform
            in SystemAPI.Query<RefRO<LocalToWorld>>()
                .WithAll<GhostOwnerIsLocal>()
        ) {
            _cameraTarget.position = transform.ValueRO.Position;
            _cameraTarget.rotation = transform.ValueRO.Rotation;
        }
    }
}
