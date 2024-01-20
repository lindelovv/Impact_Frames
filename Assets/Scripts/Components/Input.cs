using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class Input : MonoBehaviour
{
    private class Baker : Baker<Input>
    {
        public override void Bake(Input authoring)
        {
            AddComponent<InputComponent>(GetEntity(TransformUsageFlags.None));
        }
    }
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct InputComponent : IInputComponentData
{
    public float2 Value;
}
