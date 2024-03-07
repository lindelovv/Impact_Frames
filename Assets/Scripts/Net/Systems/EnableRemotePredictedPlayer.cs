using Unity.Entities;
using UnityEngine;

public struct EnableRemotePredictedPlayerComponent : IComponentData { }

[DisallowMultipleComponent]
public class EnableRemotePredictedPlayer : MonoBehaviour
{
    class Baker : Baker<EnableRemotePredictedPlayer>
    {
        public override void Bake(EnableRemotePredictedPlayer authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<EnableRemotePredictedPlayer>(entity);
        }
    }
}
