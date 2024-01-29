using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class InputSystem : SystemBase
{
    private IA_PlayerControls _inputActions;
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InputComponentData>();
        state.RequireForUpdate<NetworkId>();
    }

    protected override void OnStartRunning()
    {
        _inputActions = new IA_PlayerControls();
        _inputActions.Enable();
    }

    protected override void OnStopRunning()
    {
        _inputActions.Disable();
    }

    protected override void OnUpdate()
    {
        foreach (
            var inputData
            in SystemAPI.Query<RefRW<InputComponentData>>()
                .WithAll<GhostOwnerIsLocal>()
        ) {
            inputData.ValueRW.HorizontalMoveValue = _inputActions.Combat.Move.ReadValue<float>();
            inputData.ValueRW.Jump = _inputActions.Combat.Jump.WasPressedThisFrame();
            inputData.ValueRW.PunchValue = _inputActions.Combat.Punch.ReadValue<float>();    
        }
    }
}
