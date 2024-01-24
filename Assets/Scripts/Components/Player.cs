using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

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
            AddComponent(entity, new PlayerComponent
            {
                Prefab = entity,
                Health = authoring.Health,
                MovementSpeed = authoring.MovementSpeed,
                JumpStrength = authoring.JumpStrength,
            });
        }
    }
}

public struct PlayerComponent : IComponentData
{
    public Entity Prefab; // Needed to link prefab to in game entity
    [GhostField] public float Health;
    [GhostField] public float MovementSpeed;
    [GhostField] public float JumpStrength;
}
