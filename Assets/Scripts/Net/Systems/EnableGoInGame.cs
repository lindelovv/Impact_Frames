using Unity.Entities;
using UnityEngine;

public struct EnableGoInGameComp : IComponentData { }

[DisallowMultipleComponent]
public class EnableGoInGame : MonoBehaviour
{
    class Baker : Baker<EnableGoInGame>
    {
        public override void Bake(EnableGoInGame authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<EnableGoInGameComp>(entity);
        }
    }
}
