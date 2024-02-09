using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FrontendBootstrap : MonoBehaviour
{
    void Start()
    {
#if UNITY_SERVER
        string sceneName = "Main";
#else
        string sceneName = "Menu";
#endif
        SceneManager.LoadScene(sceneName);
    }
}
