using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

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
            var (inputData, player)

            in SystemAPI.Query<RefRW<InputComponentData>, RefRW<PlayerData>>()
                .WithAll<GhostOwnerIsLocal>()
        ) {
            inputData.ValueRW.RequestedMovement = _inputActions.Combat.Move.ReadValue<Vector2>();
            
            inputData.ValueRW.RequestJump = _inputActions.Combat.Jump.WasPressedThisFrame();
            
            if(_inputActions.Combat.Jump.WasPressedThisFrame() ) {
                // Setting coyote timeer and remember last time you were grounded
               // player.ValueRW.JumpPressRemember = player.ValueRW.JumpPressTimer;  
                
            }

            inputData.ValueRW.RequestPunch = _inputActions.Combat.Punch.WasPressedThisFrame();
            inputData.ValueRW.RequestKick = _inputActions.Combat.Kick.WasPressedThisFrame();
            inputData.ValueRW.RequestDash = _inputActions.Combat.Dash.WasPressedThisFrame();
            
            // Block/Parry update logic
            if (_inputActions.Combat.BlockParry.WasPressedThisFrame())
            {
                inputData.ValueRW.RequestBlockParry = true;
            }
            else if(_inputActions.Combat.BlockParry.WasReleasedThisFrame())
            {
                inputData.ValueRW.RequestBlockParry = false;
            }
        }
    }
}
