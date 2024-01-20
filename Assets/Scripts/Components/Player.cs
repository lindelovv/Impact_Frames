using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float Health;
    public float Speed;     //la till speed

    private class Baker : Baker<Player>
    {
        /* public override void Bake(Player authoring)
        {
            PlayerComponent player = new PlayerComponent { Health = authoring.Health, Speed = authoring.Speed}; //la till en speed för player
            
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), player);

        } */   //Gins Kod

        public override void Bake(Player authoring)         //Philips Test
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerComponent { Health = authoring.Health, Speed = authoring.Speed });
            //AddComponent(entity, new InputComponentData { });
            
        }
    }
}

public struct PlayerComponent : IComponentData
{
    [GhostField] public float Health;
    [GhostField] public float Speed;  //la till speed
}
