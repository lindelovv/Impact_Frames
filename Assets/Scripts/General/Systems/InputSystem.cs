using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class InputSystem : SystemBase
{
    private IA_PlayerControls _inputActions;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate<Input>();
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        _inputActions = new IA_PlayerControls();
        _inputActions.Enable();
    }

    protected override void OnStopRunning()
    {
        base.OnStopRunning();
        _inputActions?.Disable();
    }

    protected override void OnUpdate()
    {
        foreach (
            var (inputData, player)

            in SystemAPI.Query<RefRW<Input>, RefRW<Player>>()
                .WithAll<GhostOwnerIsLocal>()
        ) {
            ref var input = ref inputData.ValueRW;
            var action = _inputActions.Combat;
            
            inputData.ValueRW.RequestedMovement = _inputActions.Combat.Move.ReadValue<Vector2>();
            IsButtonHeld(action.Reset, ref input.RequestReset);
            IsButtonHeld(action.Jump,  ref input.RequestJump );
            IsButtonHeld(action.Punch, ref input.RequestPunch);
            IsButtonHeld(action.Kick,  ref input.RequestKick );
            IsButtonHeld(action.Dash,  ref input.RequestDash );
            IsButtonHeld(action.Block, ref input.RequestBlock);
            IsButtonHeld(action.Parry, ref input.RequestParry);
        }
    }

    void IsButtonHeld(InputAction button, ref bool state)
    {
        if (button.WasPressedThisFrame())       { state = true;  }
        else if (button.WasReleasedThisFrame()) { state = false; }
    }
}
