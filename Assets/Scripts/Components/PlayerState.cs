using System;
using System.Text;
using Unity.Entities;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Collections.AllocatorManager;
using static UnityEngine.AudioSettings;
using static UnityEngine.Rendering.DebugUI;

public class PlayerState : MonoBehaviour
{
    //-----------------------
    [Tooltip("[bool] Set if the character currently is grounded.")] // More tooltips needd?
    public bool isMoving;
    public bool isGrounded;
    public bool isMobile;
    public bool IsOnBeat;
    public bool isFacingRight;

    private class Baker : Baker<PlayerState>
    {
        public override void Bake(PlayerState authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None); // TransformUsageFlags.None för att det inte behöver synas
                                                              // i världen, till skillnad från player som är Dynamic
            var initialState = new State { };
            initialState |= authoring.isMobile      ? State.IsMobile      : 0; 
            initialState |= authoring.isGrounded    ? State.IsGrounded    : 0; 
            initialState |= authoring.isFacingRight ? State.IsFacingRight : 0; 
            initialState |= authoring.isMoving      ? State.IsMoving      : 0; 
            initialState |= authoring.IsOnBeat      ? State.IsOnBeat      : 0; 
            
            AddComponent(entity, new PlayerStateComponent {
                State = initialState,
            });
        }
    }
}

[Flags]
public enum State : UInt16
{
    /// Required for Movement, Jump, Punch, Kick, Block, Special Move, Grab, Dash, and Air Dash.
    IsMobile      = 1 << 0,
    IsGrounded    = 1 << 1,
    IsFacingRight = 1 << 2,
    IsMoving      = 1 << 3,
    IsJumping     = 1 << 4,
    IsFalling     = 1 << 5,
    IsPunching    = 1 << 6,
    IsKicking     = 1 << 7,
    /// Required Bunny Jump, Attacks 
    IsOnBeat      = 1 << 8,
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerStateComponent : IComponentData
{
    [GhostField] public State State;
}
