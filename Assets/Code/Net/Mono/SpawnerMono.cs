using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SpawnerMono : MonoBehaviour
{
    public GameObject PlayerOne;
}

public class SpawnerBaker : Baker<SpawnerMono>
{
    public override void Bake(SpawnerMono authoring)
    {
        Spawner spawner = default;
        spawner.PlayerOne = GetEntity(authoring.PlayerOne, TransformUsageFlags.Dynamic);
        AddComponent(GetEntity(TransformUsageFlags.None), spawner);
    }
}