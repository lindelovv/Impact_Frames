using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class Input : MonoBehaviour
{
    //-----------------------
    [Tooltip("[float] Current amount of health."), SerializeField]
    private float2 MoveValue;
    private class Baker : Baker<Input>
    {
        public override void Bake(Input authoring)
        {
            AddComponent<InputComponentData>(GetEntity(TransformUsageFlags.None));
        }
    }
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct InputComponentData : IInputComponentData
{

    // Movement
    public float2 MoveValue;
    
    // Jumping & DoubleJump
    public bool Jump;


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
