using Unity.Entities;
using UnityEngine;

public class PlayerMono : MonoBehaviour
{
}

public class PlayerBaker : Baker<PlayerMono>
{
    public override void Bake(PlayerMono authoring)
    {
        Player player = new Player { };
        AddComponent(GetEntity(TransformUsageFlags.Dynamic), player);
    }
}