using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

//[UpdateInGroup(typeof(InitializationSystemGroup))]
//[BurstCompile]
//public partial struct SceneInitializationSystem : ISystem
//{
//    [BurstCompile]
//    public void OnCreate(ref SystemState state)
//    {
//        state.RequireForUpdate<SpawnerComponent>();
//    }
//
//    [BurstCompile]
//    public void OnUpdate(ref SystemState state)
//    {
//        // Game init
//        if (SystemAPI.HasSingleton<SpawnerComponent>())
//        {
//            ref SpawnerComponent sceneInitializer = ref SystemAPI.GetSingletonRW<SpawnerComponent>().ValueRW;
//
//            // Cursor
//            Cursor.lockState = CursorLockMode.Locked;
//            Cursor.visible = false;
//
//            // Spawn player
//            Entity playerEntity = state.EntityManager.Instantiate(sceneInitializer.Player);
//
//            // Spawn character at spawn point
//            SystemAPI.SetComponent(playerEntity, sceneInitializer.SpawnPoint);
//
//            // Spawn camera
//            Entity cameraEntity = state.EntityManager.Instantiate(sceneInitializer.Camera);
//
//            // Assign camera & character to player
//            PlayerComponentData player = SystemAPI.GetComponent<PlayerComponentData>(playerEntity);
//            player.Camera = cameraEntity;
//            SystemAPI.SetComponent(playerEntity, player);
//            
//            state.EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<SpawnerComponent>());
//        }
//    }
//    
//    [BurstCompile] public void OnDestroy(ref SystemState state) { }
//}
