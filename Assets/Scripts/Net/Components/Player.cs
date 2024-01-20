using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float Health;
    private IA_PlayerControls playerControlls;
    private Rigidbody rb;

   
    private class Baker : Baker<Player>
    {
        public override void Bake(Player authoring)
        {
            PlayerComponent player = new PlayerComponent
            {
                Health = authoring.Health,
                
                rb = authoring.rb
               
            };
            //AddComponent(GetEntity(TransformUsageFlags.Dynamic), player);
        }
    }
}

public struct PlayerComponent : IComponentData
{
    [GhostField][SerializeField] public float Health;
    [SerializeField] public IA_PlayerControls playerControlls;
    [GhostField][SerializeField] public Rigidbody rb;

}
