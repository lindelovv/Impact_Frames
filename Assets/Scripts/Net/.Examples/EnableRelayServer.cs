using Unity.Entities;
using UnityEngine;

public class EnableRelayServer : MonoBehaviour
{
    private class Baker : Baker<EnableRelayServer>
    {
        public override void Bake(EnableRelayServer authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<EnableRelayServerTag>(entity);
        }
    }
}

public struct EnableRelayServerTag : IComponentData {}
