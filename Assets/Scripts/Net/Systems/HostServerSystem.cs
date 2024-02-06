using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;

[DisableAutoCreation]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class HostServerSystem : SystemBase
{
#if !UNITY_SERVER
    //public RelayFrontend UIBehaviour;
#endif
    const int RelayMaxConnections = 5;
    public string JoinCode;

    public RelayServerData RelayServerData;
    HostStatus _hostStatus;
    Task<List<Region>> _regionsTask;
    Task<Allocation> _allocationTask;
    Task<string> _joinCodeTask;
    Task _initTask;
    Task _signInTask;

    [Flags]
    enum HostStatus
    {
        Unknown,
        InitializeServices,
        Initializing,
        SigningIn,
        FailedToHost,
        Ready,
        GettingRegions,
        Allocating,
        GettingJoinCode,
        GetRelayData,
    }

    protected override void OnCreate()
    {
        RequireForUpdate<EnableRelayServerTag>();
        _hostStatus = HostStatus.InitializeServices;
    }

    protected override void OnUpdate()
    {
        switch (_hostStatus)
        {
        //-------------------------------------------
            case HostStatus.FailedToHost:
            {
            #if !UNITY_SERVER
                //UIBehaviour.HostConnectionStatus = "Failed, check console.";
            #endif
                _hostStatus = HostStatus.Unknown;
                return;
            }
        //-------------------------------------------
            case HostStatus.Ready:
            {
            #if !UNITY_SERVER
                //UIBehaviour.HostConnectionStatus = "Success, players may now connect.";
            #endif
                _hostStatus = HostStatus.Unknown;
                return;
            }
        //-------------------------------------------
            case HostStatus.InitializeServices:
            {
            #if !UNITY_SERVER
                //UIBehaviour.HostConnectionStatus = "Initialize services.";
            #endif
                _initTask = UnityServices.InitializeAsync();
                _hostStatus = HostStatus.Initializing;
                return;
            }
        //-------------------------------------------
            case HostStatus.Initializing:
            {
                _hostStatus = WaitForInitialization(_initTask, out _signInTask);
                return;
            }
        //-------------------------------------------
            case HostStatus.SigningIn:
            {
            #if !UNITY_SERVER
                //UIBehaviour.HostConnectionStatus = "Logging in anonymously.";
            #endif
                _hostStatus = WaitForSignIn(_signInTask, out _regionsTask);
                return;
            }
        //-------------------------------------------
            case HostStatus.GettingRegions:
            {
            #if !UNITY_SERVER
                //UIBehaviour.HostConnectionStatus = "Waiting for regions.";
            #endif
                _hostStatus = WaitForRegions(_regionsTask, out _allocationTask);
                return;
            }
        //-------------------------------------------
            case HostStatus.Allocating:
            {
            #if !UNITY_SERVER
                //UIBehaviour.HostConnectionStatus = "Waiting for allocations.";
            #endif
                _hostStatus = WaitForAllocations(_allocationTask, out _joinCodeTask);
                return;
            }
        //-------------------------------------------
            case HostStatus.GettingJoinCode:
            {
            #if !UNITY_SERVER
                //UIBehaviour.HostConnectionStatus = "Waiting for join code.";
            #endif
                _hostStatus = WaitForJoin(_joinCodeTask, out JoinCode);
                return;
            }
        //-------------------------------------------
            case HostStatus.GetRelayData:
            {
            #if !UNITY_SERVER
                //UIBehaviour.HostConnectionStatus = "Getting relay data.";
            #endif
                _hostStatus = BindToHost(_allocationTask, out RelayServerData);
                return;
            }
        //-------------------------------------------
            case HostStatus.Unknown:
            default:
            break;
        }
    }

    static HostStatus WaitForSignIn(Task signInTask, out Task<List<Region>> regionTask)
    {
        if(!signInTask.IsCompleted)
        {
            regionTask = default;
            return HostStatus.SigningIn;
        }
        if(signInTask.IsFaulted)
        {
            Debug.LogError("Signing in failed.");
            Debug.LogException(signInTask.Exception);
            regionTask = default;
            return HostStatus.FailedToHost;
        }
        regionTask = RelayService.Instance.ListRegionsAsync();
        return HostStatus.GettingRegions;
    }

    static HostStatus WaitForInitialization(Task initTask, out Task nextTask)
    {
        if(!initTask.IsCompleted)
        {
            nextTask = default;
            return HostStatus.Initializing;
        }
        if(initTask.IsFaulted)
        {
            Debug.LogError("UnityServices initalization failed.");
            Debug.LogException(initTask.Exception);
            nextTask = default;
            return HostStatus.FailedToHost;
        }
        if(AuthenticationService.Instance.IsSignedIn)
        {
            nextTask = Task.CompletedTask;
            return HostStatus.SigningIn;
        }
        else
        {
            nextTask = AuthenticationService.Instance.SignInAnonymouslyAsync();
            return HostStatus.SigningIn;
        }
    }

    static HostStatus BindToHost(Task<Allocation> allocationTask, out RelayServerData relayServerData)
    {
        var allocation = allocationTask.Result;

        try { relayServerData = HostRelayData(allocation); }
        catch(Exception e)
        {
            Debug.LogException(e);
            relayServerData = default;
            return HostStatus.FailedToHost;
        }
        return HostStatus.Ready;
    }

    static HostStatus WaitForJoin(Task<string> joinCodeTask, out string joinCode)
    {
        joinCode = null;
        if(!joinCodeTask.IsCompleted)
        {
            return HostStatus.GettingJoinCode;
        }
        if(joinCodeTask.IsFaulted)
        {
            Debug.LogError("Create join code request failed.");
            Debug.LogException(joinCodeTask.Exception);
            return HostStatus.FailedToHost;
        }
        joinCode = joinCodeTask.Result;
        return HostStatus.GetRelayData;
    }

    static HostStatus WaitForAllocations(Task<Allocation> allocationTask, out Task<string> joinCodeTask)
    {
        if(!allocationTask.IsCompleted)
        {
            joinCodeTask = null;
            return HostStatus.Allocating;
        }
        if(allocationTask.IsFaulted)
        {
            joinCodeTask = null;
            Debug.LogError("Create allocation request failed.");
            Debug.LogException(allocationTask.Exception);
            return HostStatus.FailedToHost;
        }

        var allocation = allocationTask.Result;
        joinCodeTask = RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        return HostStatus.GettingJoinCode;
    }

    static HostStatus WaitForRegions(Task<List<Region>> colletRegionTask, out Task<Allocation> allocationTask)
    {
        if(!colletRegionTask.IsCompleted)
        {
            allocationTask = null;
            return HostStatus.GettingRegions;
        }
        if(colletRegionTask.IsFaulted)
        {
            allocationTask = null;
            Debug.LogError("List regions request failed.");
            Debug.LogException(colletRegionTask.Exception);
            return HostStatus.FailedToHost;
        }

        var regionList = colletRegionTask.Result;
        var targetRegion = regionList[0].Id;

        allocationTask = RelayService.Instance.CreateAllocationAsync(RelayMaxConnections, targetRegion);
        return HostStatus.Allocating;
    }

    static RelayServerData HostRelayData(Allocation allocation, string connectionType = "dtls")
    {
        var endpoint = RelayUtilities.GetEndpointForConnectionType(allocation.ServerEndpoints, connectionType);
        if(endpoint == null)
        {
            throw new InvalidOperationException($"Endpoint for connectionType {connectionType} not found.");
        }
        var serverEndpoint = NetworkEndpoint.Parse(endpoint.Host, (ushort)endpoint.Port);

        var allocationIdBytes = RelayAllocationId.FromByteArray(allocation.AllocationIdBytes);
        var connectionData = RelayConnectionData.FromByteArray(allocation.ConnectionData);
        var key = RelayHMACKey.FromByteArray(allocation.Key);

        return new RelayServerData(
            ref serverEndpoint,
            0,
            ref allocationIdBytes,
            ref connectionData,
            ref connectionData,
            ref key,
            connectionType == "dtls"
        );
    }
}
