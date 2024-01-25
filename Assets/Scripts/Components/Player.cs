using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    //-----------------------
    [Tooltip("[float] Current amount of health.")]
    public float _health = 10;
    
    //-----------------------
    [Tooltip("[float] How fast the player can move.")]
    public float _movementSpeed = 6;
    
    //-----------------------
    [Tooltip("[float] How high the player can jump.")]
    public float _jumpStrength = 8;

    private class Baker : Baker<Player>
    {
        public override void Bake(Player authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerComponentData
            {
                Health = authoring._health,
                MovementSpeed = authoring._movementSpeed,
                JumpStrength = authoring._jumpStrength,
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

public struct PlayerName : IComponentData
{
    [GhostField] public FixedString64Bytes Name;
}

public struct LocalUser : IComponentData {}

public readonly partial struct LocalPlayerAspect : IAspect
{
    private readonly RefRW<InputComponentData> Input;
    private readonly RefRO<LocalTransform> m_LocalTransform;
    private readonly RefRW<PlayerComponentData> m_Player;
    private readonly RefRW<LocalUser> m_LocalUser;
    private readonly RefRO<LocalToWorld> m_LocalToWorld;
    public PlayerComponentData Player => m_Player.ValueRO;
    public LocalToWorld LocalToWorld => m_LocalToWorld.ValueRO;

    public bool HasValidPosition()
    {
        var isFinite = math.isfinite(m_LocalTransform.ValueRO.Position);
        return isFinite.x && isFinite.y && isFinite.z;
    }
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
