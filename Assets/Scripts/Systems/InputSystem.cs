using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial class InputSystem : SystemBase
{
    private IA_PlayerControls _inputActions = null;
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InputComponentData>();
        //state.RequireForUpdate<NetworkId>();
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
            inputData.ValueRW.MoveValue = _inputActions.Combat.Move.ReadValue<Vector2>();
            inputData.ValueRW.JumpValue = _inputActions.Combat.Jump.ReadValue<float>();
            inputData.ValueRW.PunchValue = _inputActions.Combat.Punch.ReadValue<float>();    
        }
    }
           
    // vill kolla vad som sker om man trycker punch, men det kanske borde g�ras i fightsystem eller redan i input.cs
    // Kan inte anv�nda f�r vi anv�nder inte player input och assignar unity events. 
    public void punch(InputAction.CallbackContext callbackcontext)
    {
        if (callbackcontext.performed)
        {
            Debug.Log("Punch! " +  callbackcontext);
            //G�r all logik h�r f�r att sl�
        }
    }
}
