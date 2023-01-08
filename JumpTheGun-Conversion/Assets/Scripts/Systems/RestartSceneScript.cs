using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

/**
 * This system was not successfully implemented.
 * It was supposed to restart the game when game over, but this
 *  version only unloads the entities subscene.
 */
[UpdateAfter(typeof(DamageCollisionSystem))]
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
        
        var playerEntity = GetSingleton<Player>();
        if (playerEntity.isDead)
        {
            Debug.Log("GAME OVER");
            
            m_SceneSystem.UnloadScene(sceneEntity);
            
        }

    }
}

