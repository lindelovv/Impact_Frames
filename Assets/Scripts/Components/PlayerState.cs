using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    //-----------------------
    [Tooltip("[bool] Set if the character currently is grounded.")] // More tooltips needd?
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
    /// <summary>
    /// Required specifically for Jump.
    /// </summary>
    public bool isGrounded;

    /// <summary>
    /// Required for Movement, Jump, Punch, Kick, Block, Special Move, Grab, Dash, and Air Dash.
    /// </summary>
    public bool isMobile;

    /// <summary>
    /// Required for Movement, Jump, Block, Grab, Dash, and Air Dash
    /// </summary>
    public bool IsAnimationLocked;

    /// <summary>
    /// Required Bunny Jump, Attacks 
    /// </summary>
    public bool IsOnBeat;

    /// <summary>
    /// Required for Perfect Block sucess, and incoming attack to calculate damage taken
    /// </summary>
    public bool IsAttacked;

    /// <summary>
    /// Required if Hurtbox collider is hit, to calculate damage, and Perfect Block fail
    /// </summary>
    public bool IsHit; 

    /// <summary>
    /// Required for pressing Special Move button.
    /// </summary>
    public bool HasFullComboMeterEnergy;

    /// <summary>
    /// Required for Special Move Sucess.
    /// </summary>
    public bool HasCompletedMusicalChallenge;

    /// <summary>
    /// Required if In Range of opponent, for Grab.
    /// </summary>
    public bool IsInRange;
   
    /// <summary>
    /// Required for every state that can have a cooldown
    /// </summary>
    public bool IsOnCooldown;

    /// <summary>
    /// Required for Facing the right direction
    /// </summary>
    public bool isFacingRight;
}
