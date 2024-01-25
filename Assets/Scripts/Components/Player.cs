using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

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

/*
 *  Useful for lookup, might need to use later [saved for reference of structure]
 * 
readonly partial struct PlayerAspect : IAspect
{
    public readonly Entity Self;

    private readonly RefRW<LocalTransform> Transform;
    private readonly RefRW<PlayerComponentData> Data;
    public float Health
    {
        get => Data.ValueRO.Health;
        set => Data.ValueRW.Health = value; 
    }
    public float MovementSpeed
    {
        get => Data.ValueRO.MovementSpeed;
        set => Data.ValueRW.MovementSpeed = value; 
    }
    public float JumpStrength
    {
        get => Data.ValueRO.JumpStrength;
        set => Data.ValueRW.JumpStrength = value; 
    }
}
*/
