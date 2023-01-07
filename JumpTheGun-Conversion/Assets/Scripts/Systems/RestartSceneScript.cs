using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[UpdateAfter(typeof(DestroyOnContactSystem))]
public partial class RestartSceneScript : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem ecbSystem;

    private SceneSystem m_SceneSystem;
    private EntityQuery m_NewRequests;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        m_SceneSystem = World.GetExistingSystem<SceneSystem>();
        m_NewRequests = GetEntityQuery(typeof(SceneLoader));
        RequireSingletonForUpdate<Player>();
    }

    protected override void OnUpdate()
    {
        NativeArray<SceneLoader> allocatedScenes = m_NewRequests.ToComponentDataArray<SceneLoader>(Allocator.Temp);

        var loadParameters = new SceneSystem.LoadParameters { Flags = SceneLoadFlags.DisableAutoLoad };
        var sceneEntity = m_SceneSystem.LoadSceneAsync(allocatedScenes[0].Guid, loadParameters);

        //var ecb = ecbSystem.CreateCommandBuffer();

        /*
        var aimJob = new RestartSceneJob
        {
            ecb = ecb,
            // sceneSystem = m_SceneSystem,
            // requests = allocatedScenes,
        };
        */
        //var jobHandle = aimJob.Schedule();
        //ecbSystem.AddJobHandleForProducer(jobHandle);

        //var gameData = GetSingleton<GameData>();
        var playerEntity = GetSingleton<Player>();
        if (playerEntity.isDead)
        {
            Debug.Log("GAME OVER WILL RESTART");
            
            //var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            //entityManager.DestroyEntity(entityManager.UniversalQuery);
            
            m_SceneSystem.UnloadScene(sceneEntity);
            
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
            //SceneManager.LoadScene(2);
        }

        /*
        if (gameData.gameOver)
        {
            Debug.Log("EndMe");
            m_SceneSystem.UnloadScene(sceneEntity);
            //// reloadscene
            // EntityManager.DestroyEntity(m_NewRequests);
            // m_NewRequests.Dispose();
            // m_SceneSystem.UnloadScene(sceneEntity);
        }
        */
    }
}


//[BurstCompile]
public partial struct RestartSceneJob : IJobEntity
{
    public EntityCommandBuffer ecb;

    // [BurstDiscard]
    private void Execute(Entity entity, in Player player, ref GameData gameData)
    {
        if (player.isDead)
            gameData.gameOver = true;
    }
}