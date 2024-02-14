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
    public bool IsAnimationLocked;
    public bool IsOnBeat;
    public bool IsAttacked;
    public bool IsHit;
    public bool HasFullComboMeterEnergy;
    public bool HasCompletedMusicalChallenge;
    public bool IsInRange;
    public bool IsOnCooldown;
    public bool isFacingRight;

    private class Baker : Baker<PlayerState>
    {
        public override void Bake(PlayerState authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None); // TransformUsageFlags.None för att det inte behöver synas
                                                              // i världen, till skillnad från player som är Dynamic
            AddComponent(entity, new PlayerStateComponent {
                isMoving = authoring.isMoving,
                isGrounded = authoring.isGrounded,
                isMobile = authoring.isMobile,
                IsAnimationLocked = authoring.IsAnimationLocked,
                IsOnBeat = authoring.IsOnBeat,
                IsAttacked = authoring.IsAttacked,
                IsHit = authoring.IsHit,
                HasFullComboMeterEnergy = authoring.HasFullComboMeterEnergy,
                HasCompletedMusicalChallenge = authoring.HasCompletedMusicalChallenge,
                IsInRange = authoring.IsInRange,
                IsOnCooldown = authoring.IsOnCooldown,
                isFacingRight = authoring.isFacingRight, 
            });
        }
    }
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerStateComponent : IComponentData
{
    [GhostField] public bool isMoving;
    
    [GhostField] public bool isFalling;
    
    [GhostField] public bool isJumping;
    
    /// <summary>
    /// Required specifically for Jump.
    /// </summary>
    [GhostField] public bool isGrounded;

    /// <summary>
    /// Required for Movement, Jump, Punch, Kick, Block, Special Move, Grab, Dash, and Air Dash.
    /// </summary>
    [GhostField] public bool isMobile;

    /// <summary>
    /// Required for Movement, Jump, Block, Grab, Dash, and Air Dash
    /// </summary>
    [GhostField] public bool IsAnimationLocked;

    /// <summary>
    /// Required Bunny Jump, Attacks 
    /// </summary>
    [GhostField] public bool IsOnBeat;

    /// <summary>
    /// Required for Perfect Block sucess, and incoming attack to calculate damage taken
    /// </summary>
    [GhostField] public bool IsAttacked;

    /// <summary>
    /// Required if Hurtbox collider is hit, to calculate damage, and Perfect Block fail
    /// </summary>
    [GhostField] public bool IsHit; 

    /// <summary>
    /// Required for pressing Special Move button.
    /// </summary>
    [GhostField] public bool HasFullComboMeterEnergy;

    /// <summary>
    /// Required for Special Move Sucess.
    /// </summary>
    [GhostField] public bool HasCompletedMusicalChallenge;

    /// <summary>
    /// Required if In Range of opponent, for Grab.
    /// </summary>
    [GhostField] public bool IsInRange;
   
    /// <summary>
    /// Required for every state that can have a cooldown
    /// </summary>
    [GhostField] public bool IsOnCooldown;

    /// <summary>
    /// Required for Facing the right direction
    /// </summary>
    [GhostField] public bool isFacingRight;
}
