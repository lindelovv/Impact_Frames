using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

//[BurstCompile]
//public partial struct CameraFollowSystem : ISystem
//{
//    [BurstCompile]
//    public void OnCreate(ref SystemState state)
//    {
//        state.RequireForUpdate<CameraTarget>();
//        state.RequireForUpdate<FollowerCamera>();
//    }
//
//    [BurstCompile]
//    public void OnUpdate(ref SystemState state)
//    {
//        foreach (var (follwerComponent, transform, entity)
//                 in SystemAPI.Query<FollowerCamera, RefRW<LocalTransform>>()
//                     .WithEntityAccess()
//        ) {
//            transform.ValueRW.Position = SystemAPI.GetComponent<LocalTransform>(entity).Position;
//        }
//    }
//    
//    [BurstCompile] public void OnDestroy(ref SystemState state) { }
//}
