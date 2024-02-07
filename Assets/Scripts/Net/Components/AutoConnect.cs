using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

public class AutoConnect : MonoBehaviour
{
    private class Baker : Baker<AutoConnect>
    {
        public override void Bake(AutoConnect authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<AutoConnect>(entity);
        }
    }
}

public partial struct AutoConnectTag : IComponentData {}

