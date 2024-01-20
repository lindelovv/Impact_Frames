using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class InputSystem : SystemBase
{
    private IA_PlayerControls _inputActions;
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InputComponent>();
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
        foreach(
            var input 
            in SystemAPI.Query<RefRW<InputComponent>>()
                .WithAll<GhostOwnerIsLocal>())
        {
            input.ValueRW.MoveValue = _inputActions.Combat.Move.ReadValue<Vector2>();
            input.ValueRW.JumpValue = _inputActions.Combat.Jump.ReadValue<float>();
        }
    }
}
