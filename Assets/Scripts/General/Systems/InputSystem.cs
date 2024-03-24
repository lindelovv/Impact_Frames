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
            inputData.ValueRW.RequestedMovement = _inputActions.Combat.Move.ReadValue<Vector2>();
            
            _inputActions.Combat.Reset.performed += context => inputData.ValueRW.RequestReset = true;
            _inputActions.Combat.Reset.canceled  += context => inputData.ValueRW.RequestReset = false;
            
            _inputActions.Combat.Jump.performed += context => inputData.ValueRW.RequestJump = true;
            _inputActions.Combat.Jump.canceled  += context => inputData.ValueRW.RequestJump = false;
            
            _inputActions.Combat.Punch.performed += context => inputData.ValueRW.RequestPunch = true;
            _inputActions.Combat.Punch.canceled  += context => inputData.ValueRW.RequestPunch = false;
            
            _inputActions.Combat.Kick.performed += context => inputData.ValueRW.RequestKick = true;
            _inputActions.Combat.Kick.canceled  += context => inputData.ValueRW.RequestKick = false;
            
            _inputActions.Combat.Dash.performed += context => inputData.ValueRW.RequestDash = true;
            _inputActions.Combat.Dash.canceled  += context => inputData.ValueRW.RequestDash = false;
            
            _inputActions.Combat.Block.performed += context => inputData.ValueRW.RequestBlock = true;
            _inputActions.Combat.Block.canceled  += context => inputData.ValueRW.RequestBlock = false;
            
            _inputActions.Combat.Parry.performed += context => inputData.ValueRW.RequestParry = true;
            _inputActions.Combat.Parry.canceled  += context => inputData.ValueRW.RequestParry = false;
        }
    }
}
