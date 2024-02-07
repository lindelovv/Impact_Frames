using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DecalInstanceColor : MonoBehaviour
{
    [SerializeField] Color color;
    [SerializeField] DecalProjector projector;
    private void Start()
    {
       
        projector.material = new Material(projector.material);

        projector.material.SetColor("_DecalColor", color);
    }

}
