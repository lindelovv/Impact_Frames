using Unity.Entities;
using UnityEngine;

public class OneShotEventAuthoring : MonoBehaviour
{
    class Baker : Baker<OneShotEventAuthoring>
    {
        public override void Bake(OneShotEventAuthoring authoring)
        {
            throw new System.NotImplementedException();
        }
    }
}

public struct OneShotEvent : IComponentData {}
