using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ReactionalSyncSystem : ISystem
{
    private bool _canPlayStinger;
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerStateComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (
            var playerState
            in SystemAPI.Query<RefRW<PlayerStateComponent>>()
        ) {
            var timeToBeat = Reactional.Playback.MusicSystem.GetTimeToBeat(2);
            if (timeToBeat < 1f)
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
                _canPlayStinger = true;
                playerState.ValueRW.IsOnBeat = false;
            }
        }
    }
}
