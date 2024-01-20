using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float Health;
   
    private class Baker : Baker<Player>
    {
        public override void Bake(Player authoring)
        {
            PlayerComponent player = new PlayerComponent { Health = authoring.Health };
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), player);
        }
    }
}

public struct PlayerComponent : IComponentData
{
    [GhostField] public float Health;
}
