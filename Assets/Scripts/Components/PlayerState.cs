using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    //-----------------------
    [Tooltip("[bool] Set if the character currently is grounded.")]
    public bool isGrounded;

    private class Baker : Baker<PlayerState>
    {
        public override void Bake(PlayerState authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None); // TransformUsageFlags.None för att det inte behöver synas
                                                              // i världen, till skillnad från player som är Dynamic
            AddComponent(entity, new PlayerStateComponent {
                isGrounded = authoring.isGrounded,
            });
        }
    }


}


[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerStateComponent : IComponentData
{
    // Addera alla datas för alla states, gör ett test!
    public bool isGrounded;
}

