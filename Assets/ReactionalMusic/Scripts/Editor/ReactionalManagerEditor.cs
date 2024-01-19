#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Reactional.Core;

[CustomEditor(typeof(ReactionalManager))]
public class ReactionalManagerEditor : Editor
{
    private void OnEnable()
    {
        if(Application.isPlaying){
            return;
        }
        ReactionalManager manager = (ReactionalManager)target;
        if(manager != null && manager.enabled && manager.bundles.Count == 0){
            manager.UpdateBundles(infoOnly: true);             
        }
        
    }

    public override void OnInspectorGUI()
    {
        ReactionalManager manager = (ReactionalManager)target;
        if (GUILayout.Button("Reload bundles"))
        {
            manager.UpdateBundles(infoOnly: true); 
        }
        DrawDefaultInspector();
    }
}
#endif
