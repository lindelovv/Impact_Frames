using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms.Impl;

public class Player : MonoBehaviour
{
    //-----------------------
    [Tooltip("[float] Current amount of health.")]
    public float Health = 10;
    
    //-----------------------
    [Tooltip("[float] How fast the player can move.")]
    public float MovementSpeed = 4;
    
    //-----------------------
    [Tooltip("[float] How high the player can jump.")]
    public float JumpStrength = 4;

    private class Baker : Baker<Player>
    {
        public override void Bake(Player authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerComponentData
            {
                Health = authoring.Health,
                MovementSpeed = authoring.MovementSpeed,
                JumpStrength = authoring.JumpStrength,
            });
        }
    }
}

public struct PlayerComponentData : IComponentData
{
    [GhostField] public float Health;
    [GhostField] public float MovementSpeed;
    [GhostField] public float JumpStrength;
}

readonly partial struct PlayerAspect : IAspect
{
    public readonly Entity Self;

    private readonly RefRW<LocalTransform> _transform;
    private readonly RefRW<PlayerComponentData> _data;
    public LocalTransform LocalTransform => _transform.ValueRO;
    public float Health
    {
        get => _data.ValueRO.Health;
        set => _data.ValueRW.Health = value; 
    }
    public float MovementSpeed
    {
        get => _data.ValueRO.MovementSpeed;
        set => _data.ValueRW.MovementSpeed = value; 
    }
    public float JumpStrength
    {
        get => _data.ValueRO.JumpStrength;
        set => _data.ValueRW.JumpStrength = value; 
    }
}
