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
            inputData.ValueRW.RequestedMovement.Value = _inputActions.Combat.Move.ReadValue<Vector2>();
            inputData.ValueRW.RequestJump.Value = _inputActions.Combat.Jump.IsInProgress();
            inputData.ValueRW.RequestPunch.Value = _inputActions.Combat.Punch.WasPressedThisFrame();
            
            // Block/Parry update logic
            if (_inputActions.Combat.BlockParry.WasPressedThisFrame())
            {
                inputData.ValueRW.RequestBlockParry.Value = true;
            }
            else if(_inputActions.Combat.BlockParry.WasReleasedThisFrame())
            {
                inputData.ValueRW.RequestBlockParry.Value = false;
            }
        }
    }
}
