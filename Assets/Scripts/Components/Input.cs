using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class InputData : MonoBehaviour
{
    //-----------------------
    [Tooltip("[float] Current amount of health."), SerializeField]
    private float2 MoveValue;
    private class Baker : Baker<InputData>
    {
        public override void Bake(InputData authoring)
        {
            AddComponent<InputComponentData>(GetEntity(TransformUsageFlags.None));
        }
    }
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct InputComponentData : IInputComponentData
{

    // Movement
    public float2 RequstedHorizontalMovement;
    
    // Jumping & DoubleJump
    public bool RequestJump;

    // Punch & HeavyPunch
    public bool RequestPunch;

    // Kick & HeavyKick
    public bool RequestKicking;


    // Block

    
    // Parray


    // Dash & AirDash


    // Special Attack


    // Animation Cancle
}
