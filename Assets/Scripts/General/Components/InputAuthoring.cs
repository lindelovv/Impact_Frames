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

[PredictAllNoSend]
public struct Input : IInputComponentData
{
    [GhostField(Quantization = 0)] public float2 RequestedMovement;
    [GhostField] public bool RequestJump;
    [GhostField] public bool RequestPunch;
    [GhostField] public bool RequestKick;
    [GhostField] public bool RequestBlock;
    [GhostField] public bool RequestParry;
    [GhostField] public bool RequestDash;
    [GhostField] public bool RequestReset;
}
