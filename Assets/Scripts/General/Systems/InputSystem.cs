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
        RequireForUpdate<InputData>();
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

            in SystemAPI.Query<RefRW<InputData>, RefRW<PlayerData>>()
                .WithAll<GhostOwnerIsLocal>()
        ) {
            inputData.ValueRW.RequestedMovement = _inputActions.Combat.Move.ReadValue<Vector2>();
            
            IsButtonHeld(_inputActions.Combat.Reset, ref inputData.ValueRW.RequestReset);
            IsButtonHeld(_inputActions.Combat.Jump,  ref inputData.ValueRW.RequestJump );
            IsButtonHeld(_inputActions.Combat.Punch, ref inputData.ValueRW.RequestPunch);
            IsButtonHeld(_inputActions.Combat.Kick,  ref inputData.ValueRW.RequestKick );
            IsButtonHeld(_inputActions.Combat.Dash,  ref inputData.ValueRW.RequestDash );
            //IsButtonHeld(_inputActions.Combat.Block, ref inputData.ValueRW.RequestBlock );
            
            _inputActions.Combat.Block.performed += context => inputData.ValueRW.RequestBlock = true;
            _inputActions.Combat.Block.canceled  += context => inputData.ValueRW.RequestBlock = false;

            _inputActions.Combat.Parry.performed += context => inputData.ValueRW.RequestParry = true;
            _inputActions.Combat.Parry.canceled  += context => inputData.ValueRW.RequestParry = false;
            
            //inputData.ValueRW.RequestParry = _inputActions.Combat.Parry.WasPressedThisFrame();
            
            if(_inputActions.Combat.Jump.WasPressedThisFrame() ) {
                // Setting coyote timeer and remember last time you were grounded
               // player.ValueRW.JumpPressRemember = player.ValueRW.JumpPressTimer;  
            }
        }
    }

    void IsButtonHeld(InputAction button, ref bool request)
    {
        if (button.WasPressedThisFrame())       { request = true;  }
        else if (button.WasReleasedThisFrame()) { request = false; }
    }
}
