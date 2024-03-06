using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
//[AlwaysSynchronizeSystem]
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
            
            IsButtonHeld(_inputActions.Combat.Reset,      ref inputData.ValueRW.RequestReset);
            IsButtonHeld(_inputActions.Combat.Jump,       ref inputData.ValueRW.RequestJump );
            IsButtonHeld(_inputActions.Combat.Punch,      ref inputData.ValueRW.RequestPunch);
            IsButtonHeld(_inputActions.Combat.Kick,       ref inputData.ValueRW.RequestKick );
            IsButtonHeld(_inputActions.Combat.Dash,       ref inputData.ValueRW.RequestDash );
            IsButtonHeld(_inputActions.Combat.BlockParry, ref inputData.ValueRW.RequestBlock);
            
            inputData.ValueRW.RequestParry = _inputActions.Combat.BlockParry.WasPressedThisFrame();
        }
    }

    void IsButtonHeld(InputAction button, ref bool request)
    {
        if (button.WasPressedThisFrame())       { request = true;  }
        else if (button.WasReleasedThisFrame()) { request = false; }
    }
}
