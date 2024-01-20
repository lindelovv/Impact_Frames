using System.ComponentModel;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Input : MonoBehaviour
{
    [SerializeField] public float movementSpeed = 4;
    [SerializeField] public float jumpStrength = 4;
    private class Baker : Baker<Input>
    {
        public override void Bake(Input authoring)
        {
            InputComponentData inputData = new InputComponentData
            {
                MovementSpeed = authoring.movementSpeed,
                JumpStrength = authoring.jumpStrength,
            };
            AddComponent(GetEntity(TransformUsageFlags.None), inputData);
        }
    }
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct InputComponentData : IInputComponentData
{

    // Movement
    public float MovementSpeed;  // byt ut mot player speed variabeln i Player.cs
    public float2 MoveValue;
    
    // Jumping & DoubleJump
    public float JumpValue;
    public float JumpStrength;
    public bool isJumping;


    // Punch & HeavyPunch
    public bool isPunching;
    public float PunchValue;


    // Kick & HeavyKick
    public bool isKicking;


    // Block

    
    // Parray


    // Dash & AirDash


    // Special Attack


    // Animation Cancle
}
