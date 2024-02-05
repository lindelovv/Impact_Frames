using System;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

[DisableAutoCreation]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class ConnectingPlayerSystem : SystemBase
{
    Task<JoinAllocation> _joinTask;
    Task _setupTask;
    ClientStatus _clientStatus;
    string _replayJoinCode;
    NetworkEndpoint _endpoint;
    NetworkConnection _clentConnection;
    public RelayServerData RelayClientData;

#if !UNITY_SERVER
    public RelayFrontend UIBehaviour;
#endif

    [Flags]
    enum ClientStatus {
        Unknown,
        FailedToConnect,
        Ready,
        GetJoinCodeFromHost,
        WaitForJoin,
        WaitForInit,
        WaitForSignIn,
    }

    protected override void OnCreate()
    {
        RequireForUpdate<EnableRelayServer>();
        _clientStatus = ClientStatus.Unknown;
    }

    public void GetJoinCodeFromHost()
    {
        _clientStatus = ClientStatus.GetJoinCodeFromHost;
    }

    public void JoingUsingCode(string joinCode)
    {
#if !UNITY_SERVER //-------------------------------------------
        UIBehaviour.ClientConnectionStatus = "Waiting for relay response.";
#endif //------------------------------------------------------
        _replayJoinCode = joinCode;
        _setupTask = UnityServices.InitializeAsync();
        _clientStatus = ClientStatus.WaitForInit;
    }

    protected override void OnUpdate()
    {
        switch(_clientStatus)
        {
        //-------------------------------------------
            case ClientStatus.Ready:
            {
            #if !UNITY_SERVER
                UIBehaviour.ClientConnectionStatus = "Success.";
            #endif
                _clientStatus = ClientStatus.Unknown;
                return;
            }

        //-------------------------------------------
            case ClientStatus.FailedToConnect:
            {
            #if !UNITY_SERVER
                UIBehaviour.ClientConnectionStatus = "Failed, check console.";
            #endif
                _clientStatus = ClientStatus.Unknown;
                return;
            }

        //-------------------------------------------
            case ClientStatus.GetJoinCodeFromHost: 
            {
            #if !UNITY_SERVER
                UIBehaviour.ClientConnectionStatus = "Waiting for join code from host server.";
            #endif
                var hostServer = World.GetExisitingSystemManaged<HostServer>();
                _clientStatus = JoingUsingJoinCode(hostServer.JoinCode, out _joinTask);
                return;
            }

        //-------------------------------------------
            case ClientStatus.WaitForJoin: 
            {
            #if !UNITY_SERVER
                UIBehaviour.ClientConnectionStatus = "Binding to relay server.";
            #endif
                _clientStatus = WaitForJoin(_joinTask, out RelayClientData);
                return;
            }

        //-------------------------------------------
            case ClientStatus.WaitForInit:
            {
                if(_setupTask.IsCompleted)
                {
                    if(!AuthenticationService.Instance.IsSignedIn)
                    {
                        _setupTask = AuthenticationService.Instance.SignInAnonymouslyAsync();
                        _clientStatus = ClientStatus.WaitForSignIn;
                    }
                }
                return;
            }

        //-------------------------------------------
            case ClientStatus.WaitForSignIn:
            {
                if(_setupTask.IsCompleted)
                {
                    _clientStatus = JoingUsingJoinCode(_replayJoinCode, out _joinTask);
                }
                return;
            }

        //-------------------------------------------
            case ClientStatus.Unknown:
            default:
            break;
        }
    }

    static ClientStatus WaitForJoin(Task<JoinAllocation> joinTask, out RelayServerData relayClientData)
    {
        if(!joinTask.IsCompleted)
        {
            relayClientData = default;
            return ClientStatus.WaitForJoin;
        }
        if(joinTask.IsFaulted)
        {
            relayClientData = default;
            Debug.LogError("Join Relay request failed.");
            Debug.LogException(joinTask.Exception);
            return ClientStatus.FailedToConnect;
        }
        return BindToRelay(joinTask, out relayClientData);
    }

    static ClientStatus BindToRelay(Task<JoinAllocation> joinTask, out RelayServerData relayClientData)
    {
        var allocation = joinTask.Result;

        try { relayClientData = PlayerRelayData(allocation); }
        catch(Exception e)
        {
            Debug.LogException(e);
            relayClientData = default;
            return ClientStatus.FailedToConnect;
        }
        return ClientStatus.Ready;
    }

    static ClientStatus JoingUsingJoinCode(string hostServerJoinCode, out Task<JoinAllocation> joinTask)
    {
        if(hostServerJoinCode == null)
        {
            joinTask = null;
            return ClientStatus.GetJoinCodeFromHost;
        }
        joinTask = RelayService.Instance.JoinAllocationAsync(hostServerJoinCode);
        return ClientStatus.WaitForJoin;
    }

    static RelayServerData PlayerRelayData(JoinAllocation allocation, string connectionType = "dtls")
    {
        var endpoint = RelayUtilities.GetEndpointForConnectionType(allocation.ServerEndpoints, connectionType);
        if(endpoint == null)
        {
            throw new Exception($"Endpoint for connectionType {connectionType} not found.");
        }
        var serverEndpoint = NetworkEndpoint.Parse(enpoint.Host, (ushort)endpoint.Port);

        var aloocationIdBytes = RelayAllocationId.FromByteArray(allocation.AllocationIdBytes);
        var connectionData = RelayConnectionData.FromByteArray(allocation.ConnectionData);
        var hostConnectionData = RelayConnectionData.FromByteArray(allocation.HostConnectionData);
        var key = RelayHMACKey.FromByteArray(allocation.Key);

        return new RelayServerData(
            ref serverEndpoint,
            0,
            ref aloocationIdBytes,
            ref connectionData,
            ref hostConnectionData,
            ref key,
            connectionType == "dtls"
        );
    }
}

