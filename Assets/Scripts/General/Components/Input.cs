using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class Input : MonoBehaviour
{
    //-----------------------
    [Tooltip("[float2] Movement requested from { w, a, s, d } keypresses."), SerializeField]
    private float2 RequestedMove;
    
    private class Baker : Baker<Input>
    {
        public override void Bake(Input authoring)
        {
            AddComponent<InputData>(GetEntity(TransformUsageFlags.None));
        }
    }
}

//[GhostComponent(
//    PrefabType = GhostPrefabType.AllPredicted,
//    OwnerSendType = SendToOwnerType.SendToNonOwner
//)]
[GhostComponent(
    PrefabType = GhostPrefabType.All,
    SendTypeOptimization = GhostSendType.AllClients,
    OwnerSendType = SendToOwnerType.SendToNonOwner
)]
public struct InputData : IInputComponentData
{
    // Movement
    [GhostField] public float2 RequestedMovement;
    
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
