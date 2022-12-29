using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Scenes;
using UnityEngine;

[UpdateAfter(typeof(DestroyOnContact))]
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
    }

    protected override void OnUpdate()
    {
        NativeArray<SceneLoader> allocatedScenes = m_NewRequests.ToComponentDataArray<SceneLoader>(Allocator.Temp);

        var loadParameters = new SceneSystem.LoadParameters { Flags = SceneLoadFlags.DisableAutoLoad };
        var sceneEntity = m_SceneSystem.LoadSceneAsync(allocatedScenes[0].Guid, loadParameters);
        //// m_SceneSystem.UnloadScene(sceneEntity);

        var ecb = ecbSystem.CreateCommandBuffer();

        var aimJob = new RestartSceneJob
        {
            ecb = ecb,
            done = false
            // sceneSystem = m_SceneSystem,
            // requests = allocatedScenes,
        };
        var jobHandle = aimJob.Schedule();
        ecbSystem.AddJobHandleForProducer(jobHandle);
        // //kill, then reload
        // Debug.Log("EndMe");
        // m_SceneSystem.UnloadScene(sceneEntity);
        // Debug.Log("RevMe");
        // m_SceneSystem.LoadSceneAsync(sceneEntity);
        //// not working at the moment
        // if (aimJob.done)
        // {
        //     Debug.Log("EndMe");
        //     m_SceneSystem.UnloadScene(sceneEntity);
        //     //// reloadscene
        //     // EntityManager.DestroyEntity(m_NewRequests);
        //     // m_NewRequests.Dispose();
        //     // m_SceneSystem.UnloadScene(sceneEntity);
        // }


        // // var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        // // entityManager.DestroyEntity(entityManager.UniversalQuery);
        // // SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }
}


[BurstCompile]
public partial struct RestartSceneJob : IJobEntity
{
    public EntityCommandBuffer ecb;

    public bool done;

    // [BurstDiscard]
    private void Execute(Entity entity, ref Player player, ref Health health)
    {
        if (player.isDead && health.Value <= 5)
        {
            // sceneSystem.LoadSceneAsync(requests[0].Guid);
            // done = true;
            // Debug.Log("EndMe");
            // ecb.DestroyEntity(entity);
            // sceneSystem.UnloadScene(scene);
        }
    }
}