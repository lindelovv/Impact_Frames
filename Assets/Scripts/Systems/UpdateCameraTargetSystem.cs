
using Unity.Entities;
using UnityEngine;

public partial class UpdateCameraTargetSystem : SystemBase
{
    private Transform _cameraTarget;

    protected override void OnCreate()
    {
        RequireForUpdate<LocalUser>();
    }

    protected override void OnStartRunning()
    {
        _cameraTarget = GameObject.FindWithTag("CameraTarget").transform;
        base.OnStartRunning();
    }

    protected override void OnUpdate()
    {
        foreach (var localPlayer in SystemAPI.Query<LocalPlayerAspect>())
        {
            _cameraTarget.position = localPlayer.LocalToWorld.Position;
            _cameraTarget.rotation = localPlayer.LocalToWorld.Rotation;
        }
    }
}