using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation, WorldSystemFilterFlags.ClientSimulation)]
[AlwaysSynchronizeSystem]
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
            inputData.ValueRW.RequestedMovement = _inputActions.Combat.Move.ReadValue<Vector2>();
            if (_inputActions.Combat.Jump.IsInProgress()) { inputData.ValueRW.RequestJump.Set(); }
            if (_inputActions.Combat.Punch.WasPressedThisFrame()) { inputData.ValueRW.RequestPunch.Set(); }
            if (_inputActions.Combat.Kick.WasPressedThisFrame()) { inputData.ValueRW.RequestKick.Set(); }
            if (_inputActions.Combat.BlockParry.WasPressedThisFrame()) { inputData.ValueRW.RequestParry.Set(); }
            // Block/Parry update logic
            if (_inputActions.Combat.BlockParry.WasPressedThisFrame())
            {
                inputData.ValueRW.RequestBlock = true;
            }
            else if(_inputActions.Combat.BlockParry.WasReleasedThisFrame())
            {
                inputData.ValueRW.RequestBlock = false;
            }
        }
    }
}
