using Unity.NetCode;
using UnityEngine.Scripting;
using UnityEngine.SceneManagement;

[Preserve]
public class NetcodeBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
#if UNITY_EDITOR
        var sceneName = SceneManager.GetActiveScene().name;
        var isFrontend = sceneName == "Main";
        if (RequestedPlayType == PlayType.Server && isFrontend)
        {
            AutoConnectPort = 7979;
            CreateServerWorld("Server");
            SceneManager.LoadScene("Main");
            return true;
        }
        if (isFrontend)
        {
            AutoConnectPort = 0;
            CreateLocalWorld(defaultWorldName);
        }
        else
        {
            AutoConnectPort = 7979;
            CreateDefaultClientServerWorlds();
        }
#elif UNITY_SERVER
        AutoConnectPort = 7979;
        CreateServerWorld("Server");
#else
        CreateLocalWorld(defaultWorldName);
#endif
        return true;
    }
}
