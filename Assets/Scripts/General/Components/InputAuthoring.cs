using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class InputAuthoring : MonoBehaviour
{
    [Tooltip("[float2] Movement requested from { w, a, s, d } keypresses."), SerializeField]
    private float2 RequestedMove;

    private class Baker : Baker<InputAuthoring>
    {
        public override void Bake(InputAuthoring authoring)
        {
            AddComponent<Input>(GetEntity(TransformUsageFlags.None));
        }
    }
}

[GhostComponent(
    PrefabType           = GhostPrefabType.AllPredicted,
    SendTypeOptimization = GhostSendType.AllClients,
    OwnerSendType        = SendToOwnerType.SendToNonOwner)]
public struct Input : IInputComponentData
{
    // Movement
    [GhostField(Quantization = 0)] public float2 RequestedMovement;
    
    // Jumping & DoubleJump
    [GhostField] public bool RequestJump;

    // Punch & HeavyPunch
    [GhostField] public bool RequestPunch;

    // Kick & HeavyKick
    [GhostField] public bool RequestKick;

    // Block
    [GhostField] public bool RequestBlock;
    
    // Parry
    [GhostField] public bool RequestParry;

    // Dash & AirDash
    [GhostField] public bool RequestDash;

    // Special Attack

    // Animation Cancle
    
    //
    [GhostField] public bool RequestReset;
}
