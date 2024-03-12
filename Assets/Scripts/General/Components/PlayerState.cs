using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    //-----------------------
    [Tooltip("[bool] Set if the character currently is grounded.")] // More tooltips needd?
    public bool isMoving;
    public bool isGrounded;
    public bool isMobile;
    //public int HitCounter;
    public bool IsOnBeat;
    public bool IsAttacked;
    public bool HasFullComboMeterEnergy;
    public bool HasCompletedMusicalChallenge;
    public bool IsInRange;
    public bool IsOnCooldown;
    public bool isFacingRight;
    public bool IsHit;
    
    private class Baker : Baker<PlayerState>
    {
        public override void Bake(PlayerState authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None); // TransformUsageFlags.None för att det inte behöver synas
                                                              // i världen, till skillnad från player som är Dynamic
            AddComponent(entity, new PlayerStateComponent {
                IsMoving = authoring.isMoving,
                IsGrounded = authoring.isGrounded,
                IsMobile = authoring.isMobile,
                //HitCounter = authoring.HitCounter,
                IsOnBeat = authoring.IsOnBeat,
                IsAttacked = authoring.IsAttacked,
                HasFullComboMeterEnergy = authoring.HasFullComboMeterEnergy,
                HasCompletedMusicalChallenge = authoring.HasCompletedMusicalChallenge,
                IsInRange = authoring.IsInRange,
                IsOnCooldown = authoring.IsOnCooldown,
                IsFacingRight = authoring.isFacingRight,
                IsHit = authoring.IsHit,
            });
        }
    }
}

[GhostComponent(
    PrefabType=GhostPrefabType.AllPredicted,
    OwnerSendType = SendToOwnerType.SendToNonOwner
)]
public struct PlayerStateComponent : IComponentData
{
    [GhostField] public bool IsFallingHigh;  // When Raytracing from high

    [GhostField] public bool IsMoving;
    
    [GhostField] public bool IsFalling;
    
    [GhostField] public bool IsJumping;
    
    [GhostField] public bool IsPunching;
    
    [GhostField] public bool IsKicking;
    
    [GhostField] public bool IsBlocking;
    
    [GhostField] public bool IsParrying;
    
    [GhostField] public bool IsDashing;

    /// <summary> Required for Movement, Jump, Block, Grab, Dash, and Air Dash </summary>
    [GhostField] public bool IsAnimLocked;

    /// <summary> Required specifically for Jump. </summary>
    [GhostField] public bool IsGrounded;

    /// <summary> Required for Movement, Jump, Punch, Kick, Block, Special Move, Grab, Dash, and Air Dash. </summary>
    [GhostField] public bool IsMobile;

    /// <summary> Counting Hits and Kicks </summary>
    [GhostField] public int HitCounter;
    
    /// <summary> Counting time since last attack </summary>
    [GhostField] public float HitTime;

    /// <summary> Required Bunny Jump, Attacks </summary>
    [GhostField] public bool IsOnBeat;

    /// <summary> Required for Perfect Block sucess, and incoming attack to calculate damage taken </summary>
    [GhostField] public bool IsAttacked;

    /// <summary> Required if Hurtbox collider is hit, to calculate damage, and Perfect Block fail </summary>
    [GhostField] public bool IsHit; 

    /// <summary> Required for pressing Special Move button. </summary>
    [GhostField] public bool HasFullComboMeterEnergy;

    /// <summary> Required for Special Move Sucess. </summary>
    [GhostField] public bool HasCompletedMusicalChallenge;

    /// <summary> Required if In Range of opponent, for Grab. </summary>
    [GhostField] public bool IsInRange;
   
    /// <summary> Required for every state that can have a cooldown </summary>
    [GhostField] public bool IsOnCooldown;

    /// <summary> Required for Facing the right direction </summary>
    [GhostField] public bool IsFacingRight;
    
    [GhostField] public int Random;
}
