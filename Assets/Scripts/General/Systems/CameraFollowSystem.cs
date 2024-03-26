using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(TransformSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class UpdateCameraTargetSystem : SystemBase
{
    private Transform _cameraTarget;
    private Transform _playerOne;
    private Transform _playerTwo;

    protected override void OnCreate()
    {
        RequireForUpdate<Input>();
        RequireForUpdate<NetworkId>();
    }

    protected override void OnStartRunning()
    {
        _cameraTarget = GameObject.FindWithTag("CameraTarget").transform;
        _playerOne = GameObject.FindWithTag("PlayerOne").transform;
        _playerTwo = GameObject.FindWithTag("PlayerTwo").transform;
        base.OnStartRunning();
    }

    protected override void OnUpdate()
    {
        foreach (
            var transform
            in SystemAPI.Query<RefRO<LocalToWorld>>()
                .WithAll<GhostOwnerIsLocal>()
        ) {
            var camPos = transform.ValueRO.Position;
            _cameraTarget.position = new float3(camPos.x, camPos.y + 1, camPos.z);
            _cameraTarget.rotation = transform.ValueRO.Rotation;
        }
        foreach (
            var (transform, id)
            in SystemAPI.Query<RefRO<LocalToWorld>, PlayerId>()
        ) {
            if(id.Value == 0) { continue; }
            var playerPos = transform.ValueRO.Position;
            Debug.Log($"{id.Value}");
            switch (id.Value)
            {
                case 1:
                {
                    _playerOne.position = new float3(playerPos.x, playerPos.y + 1, playerPos.z);
                    _playerOne.rotation = transform.ValueRO.Rotation;
                    break;
                }
                case 2:
                {
                    _playerTwo.position = new float3(playerPos.x, playerPos.y + 1, playerPos.z);
                    _playerTwo.rotation = transform.ValueRO.Rotation;
                    break;
                }
            }
        }
    }
}
