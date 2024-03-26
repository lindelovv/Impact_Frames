using System;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;

// Some extra custom made math utility functions
public struct Util {
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float moveTowards(float current, float target, float maxDistanceDelta)
    {
      float num = target - current;
      float d = (num * num);
      if (d == 0.0 || maxDistanceDelta >= 0.0 && d <= maxDistanceDelta * maxDistanceDelta) { return target; }
      float num2 = math.sqrt(d);
      return (current + num / num2 * maxDistanceDelta);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Entity ColliderCast(CollisionWorld collisionWorld, PhysicsCollider collider, float3 from, float3 to)
    {
        ColliderCastHit hit = new ColliderCastHit();
        
        bool haveHit = collisionWorld.CastCollider(new ColliderCastInput {
            Collider = collider.ColliderPtr,
            Start = from,
            End = to,
        }, out hit);
        
        return hit.Entity;
    }
}

[GhostComponent(
    PrefabType           = GhostPrefabType.AllPredicted  ,
    SendTypeOptimization = GhostSendType  .AllClients    ,
    OwnerSendType        = SendToOwnerType.SendToNonOwner)]
public class PredictAll : Attribute {}

[GhostComponent(
    PrefabType           = GhostPrefabType.AllPredicted,
    SendTypeOptimization = GhostSendType  .AllClients  ,
    OwnerSendType        = SendToOwnerType.None        )]
public class PredictAllNoSend : Attribute {}
