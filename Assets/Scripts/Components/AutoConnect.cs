using Unity.Entities;
using UnityEngine;

public class AutoConnect : MonoBehaviour
{
    private class Baker : Baker<AutoConnect>
    {
        public override void Bake(AutoConnect authoring)
        {
            var entity = GetEntity(authoring.gameObject, TransformUsageFlags.None);
            AddComponent<AutoConnectComponent>(entity);
        }
    }
}

public struct AutoConnectComponent : IComponentData { }
