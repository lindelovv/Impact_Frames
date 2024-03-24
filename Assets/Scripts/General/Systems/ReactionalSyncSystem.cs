using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ReactionalSyncSystem : ISystem
{
    private bool _canPlayStinger;
    private static readonly float _gracePeriod = 0.5f;
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerState>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (
            var playerState
            in SystemAPI.Query<RefRW<PlayerState>>()
        ) {
            var timeToBeat = Reactional.Playback.MusicSystem.GetTimeToBeat(2);
            if (timeToBeat < _gracePeriod )
            {
                playerState.ValueRW.IsOnBeat = true;
                if (playerState.ValueRO is { IsJumping: true, IsGrounded: false } && _canPlayStinger)
                {
                    _canPlayStinger = false;
                    Reactional.Playback.Theme.TriggerStinger("medium");
                }
            }
            else
            {
                if (!playerState.ValueRO.IsGrounded) { _canPlayStinger = true; }
                playerState.ValueRW.IsOnBeat = false;
            }
        }
    }
}
