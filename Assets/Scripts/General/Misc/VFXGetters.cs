using UnityEngine;
using UnityEngine.VFX;

public class VFXGetters : MonoBehaviour
{
    public VisualEffect Block;
    public VisualEffect DashSmoke;
    public VisualEffect SmokeTrail;
    public VisualEffect LandingImpact;

    

    private void OnEnable()
    {
        //MyVFXTransformBinder myVFXTransformBinder = DashTrail.GetComponent<MyVFXTransformBinder>();
        //myVFXTransformBinder.Target = Target.transform;
        //Target = GameObject.FindWithTag("CameraTarget");
        //if (!Target)
        //{
        //    Debug.Log("Target null");
        //}
    }
}
