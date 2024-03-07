using UnityEngine;
using UnityEngine.VFX;

public class Test: MonoBehaviour
{
    public GameObject Target;
    public VisualEffect DashTrail;
    
    private void Start()
    {
        MyVFXTransformBinder myVFXTransformBinder = DashTrail.GetComponent<MyVFXTransformBinder>();
        myVFXTransformBinder.Target = Target.transform;
    }
}
