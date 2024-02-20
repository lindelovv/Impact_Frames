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
    [GhostField] public Vec2Command RequestedMovement;
    
    // Jumping & DoubleJump
    [GhostField] public BoolCommand RequestJump;

    // Punch & HeavyPunch
    [GhostField] public BoolCommand RequestPunch;

    // Kick & HeavyKick
    [GhostField] public BoolCommand RequestKick;

    // Block
    [GhostField] public BoolCommand RequestBlockParry;

    // Dash & AirDash


    // Special Attack


    // Animation Cancle
}

public struct Vec2Command : ICommandData
{
    [GhostField] public NetworkTick Tick { get; set; }
    [GhostField(Quantization=100)] public float2 Value;
}

public struct BoolCommand : ICommandData
{
    [GhostField] public NetworkTick Tick { get; set; }
    [GhostField] public bool Value;
}
