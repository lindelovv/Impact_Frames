using System.ComponentModel;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class Input : MonoBehaviour
{
    [SerializeField] public float movementSpeed = 4;
    [SerializeField] public float jumpStrength = 4;
    private class Baker : Baker<Input>
    {
        public override void Bake(Input authoring)
        {
            InputComponent input = new InputComponent
            {
                MovementSpeed = authoring.movementSpeed,
                JumpStrength = authoring.jumpStrength,
            };
            AddComponent(GetEntity(TransformUsageFlags.None), input);
        }
    }
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct InputComponent : IInputComponentData
{
    // Movement
    public float MovementSpeed;
    public float2 MoveValue;
    
    // Jumping
    public float JumpValue;
    public float JumpStrength;
}
