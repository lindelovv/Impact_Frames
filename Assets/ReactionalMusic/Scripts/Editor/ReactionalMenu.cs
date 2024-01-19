#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using Reactional.Core;

[InitializeOnLoad]
public class ReactionalEditor : MonoBehaviour
{
    const string dontAskAgainKey = "Reactional.DontShowAgain";
    private static bool firstTime = true;

    static ReactionalEditor()
    {
        // check if folder exists, otherwise create it
        if (!Directory.Exists(Application.streamingAssetsPath + "/Reactional"))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath + "/Reactional");
        }
        EditorApplication.hierarchyChanged += CheckManager;
    }
    static void CheckManager()
    {
        if (firstTime && EditorPrefs.GetBool(Application.productName + "." + dontAskAgainKey, true))
        {
            //EditorPrefs.SetBool(firstTimeKey, false);
            var rm = FindObjectOfType<ReactionalManager>();
            if (rm == null)
            {
                int option = EditorUtility.DisplayDialogComplex("Welcome to Reactional", "Would you like to add Reactional to the current scene?", "Yes", "No", "Don't show again");
                switch (option)
                {
                    case 0:
                        AddReactionalManager();
                        break;
                    case 2:
                        EditorPrefs.SetBool(Application.productName + "." + dontAskAgainKey, false);
                        break;
                }
            }
        }
        firstTime = false;
    }


    [MenuItem("Reactional/Add Reactional Manager")]
    static void AddReactionalManager()
    {
        var rm = FindObjectOfType<ReactionalManager>();
        if (rm != null)
        {
            Debug.LogWarning("Reactional Manager already exists in the scene.");
            return;
        }
        // Create the Reactional Music GameObject and add the ReactionalManager and BasicPlayback components
        GameObject reactionalMusic = new GameObject("Reactional Music");
        reactionalMusic.AddComponent<ReactionalManager>();
        reactionalMusic.AddComponent<BasicPlayback>();

        // Create the Reactional Engine child GameObject and add the ReactionalEngine script
        GameObject reactionalEngine = new GameObject("Reactional Engine");
        reactionalEngine.AddComponent<ReactionalEngine>();
        reactionalEngine.transform.SetParent(reactionalMusic.transform);
    }

    [MenuItem("Reactional/Visit Reactional For Developers", false, 98)]
    static void OpenReactionalWebsite()
    {
        Application.OpenURL("https://app.reactionalmusic.com/");
    }

    [MenuItem("Reactional/Help", false, 99)]
    static void OpenReactionalManual()
    {
        Application.OpenURL("https://www.notion.so/reactional/How-to-add-Reactional-to-a-scene-5567ac13513a4418a94db911bb8aaf2e?pvs=4");
    }
}
#endif