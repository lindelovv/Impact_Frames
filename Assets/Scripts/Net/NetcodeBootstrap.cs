using Unity.NetCode;
using UnityEngine.Scripting;

[Preserve]
public class NetcodeBootstrap : ClientServerBootstrap
{
    public override bool Initialize(string defaultWorldName)
    {
        AutoConnectPort = 7979;                     // Enable auto connect
        return base.Initialize(defaultWorldName);   // Use the regular bootstrap
    }
}
