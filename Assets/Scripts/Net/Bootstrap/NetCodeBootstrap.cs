using System;
using Unity.NetCode;

[UnityEngine.Scripting.Preserve]
public class NetCodeBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
#if UNITY_EDITOR
        var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isFrontend = sceneName == "Frontend";
#elif !FRONTEND_PLAYER_BUILD
        bool isFrontend = false;
#endif

#if UNITY_EDITOR || !FRONTEND_PLAYER_BUILD
        if (!isFrontend)
        {
            AutoConnectPort = 7979;
            CreateDefaultClientServerWorlds();
        }
        else
        {
            AutoConnectPort = 0;
            CreateLocalWorld(defaultWorldName);
        }
#else
        CreateLocalWorld(defaultWorldName);
#endif
        return true;
    }
}
