

using Unity.Entities;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject Player;
    
    private class Baker : Baker<Spawner>
    {
        public override void Bake(Spawner authoring)
        {
            SpawnerComponent spawner = default;
            spawner.Player = GetEntity(authoring.Player, TransformUsageFlags.Dynamic);
            AddComponent(GetEntity(TransformUsageFlags.None), spawner);
        }
    }
}

public struct SpawnerComponent : IComponentData
{
    public Entity Player;
}
