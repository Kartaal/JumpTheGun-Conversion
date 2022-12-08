using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(SpawnTerrainSystem))]
public partial class CannonballSpawningSystem : SystemBase
{
    public BeginSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        if (UnityEngine.Time.frameCount % 240 == 0) // Shitty fire rate limitation for now...
        {
            Entity cannonballPrefab = GetSingleton<CannonballPrefab>().entity;

            var cannonballSpawnJob = new CannonballSpawnJob
            {
                ecb = ecbSystem.CreateCommandBuffer(),
                prefab = cannonballPrefab
            };

            var handle = cannonballSpawnJob.Schedule();
        
            ecbSystem.AddJobHandleForProducer(handle);
        }
        
        

    }
}


[BurstCompile]
[WithAll(typeof(Tank))]
public partial struct CannonballSpawnJob : IJobEntity
{
    public EntityCommandBuffer ecb;
    public Entity prefab;
    
    public void Execute(in Translation translation)
    {
        Entity cannonball = ecb.Instantiate(prefab);
        
        ecb.SetComponent(cannonball, new Translation
        {
            Value = translation.Value
        });
    }
}
