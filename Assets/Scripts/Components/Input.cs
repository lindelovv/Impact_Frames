using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class InputData : MonoBehaviour
{
    //-----------------------
    [Tooltip("[float2] Movement requested from { w, a, s, d } keypresses."), SerializeField]
    private float2 RequestedMove;
    
    private class Baker : Baker<InputData>
    {
        public override void Bake(InputData authoring)
        {
            AddComponent<InputComponentData>(GetEntity(TransformUsageFlags.None));
        }
    }
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.SendToNonOwner)]
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
