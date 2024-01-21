using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float Health = 10;
    public float MovementSpeed = 4;     //la till speed
    public float JumpStrength = 4;

    private class Baker : Baker<Player>
    {
        public override void Bake(Player authoring)         //Philips Test
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerComponent
            {
                Health = authoring.Health,
                MovementSpeed = authoring.MovementSpeed,
                JumpStrength = authoring.JumpStrength,
            });
            //AddComponent(entity, new InputComponentData { });
        }
    }
}

public struct PlayerComponent : IComponentData
{
    [GhostField] public float Health;
    [GhostField] public float MovementSpeed;  //la till speed
    [GhostField] public float JumpStrength;
}
