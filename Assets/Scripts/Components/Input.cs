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
    [GhostField] public float2 RequestedMovement;
    
    // Jumping & DoubleJump
    [GhostField] public bool RequestJump;

    // Punch & HeavyPunch
    [GhostField] public bool RequestPunch;

    // Kick & HeavyKick
    public bool RequestKick;


    // Block

    
    // Parray


    // Dash & AirDash


    // Special Attack


    // Animation Cancle
}
