using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public float Speed = 5.0f;
    public float MaxSpeed = 7.5f;
    
    public float JumpStrength = 0.15f;
    
    public float3 Gravity = new float3(0.0f, -9.81f, 0.0f);
    public float Drag = 0.2f;

    public float MaxStep = 0.35f;

    private class Baker : Baker<CharacterController>
    {
        public override void Bake(CharacterController authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new CharacterControllerData {
                Gravity = authoring.Gravity,
                MaxSpeed = authoring.MaxSpeed,
                Speed = authoring.MaxSpeed,
                JumpStrength = authoring.JumpStrength,
                MaxStep = authoring.MaxSpeed,
                Drag = authoring.Drag,
            });
        }
    }
}

public struct CharacterControllerData : IComponentData
{
    public float3 currentDirection { get; set; }
    public float3 currentMagnitude { get; set; }
    
    public bool   Jump         { get; set; }
    public float  JumpStrength { get; set; }
    public float3 JumpVelocity { get; set; }
    
    
    public float  MaxSpeed { get; set; }
    public float  Speed    { get; set; }
    
    public float MaxStep { get; set; }
    public float Drag    { get; set; }
    public float3 Gravity  { get; set; }
}
