using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

public class TimeoutServer : MonoBehaviour
{
    private class Baker : Baker<TimeoutServer>
    {
        public override void Bake(TimeoutServer authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<TimeoutServerData>(entity);
        }
    }
}

public partial struct TimeoutServerData : IComponentData
{
    public double Value;
}

