using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial class SpawnerSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        Entity cannonballPrefab = GetSingleton<CannonballPrefab>().entity;

        var cannonballSpawnJob = new CannonSpawner
        {
            ecb = ecbSystem.CreateCommandBuffer(),
            prefab = cannonballPrefab,
            deltaTime = Time.DeltaTime,
        };
        
        var handle = cannonballSpawnJob.Schedule();
        
        ecbSystem.AddJobHandleForProducer(handle);
    }
}


[BurstCompile]
public partial struct CannonSpawner : IJobEntity
{
    public EntityCommandBuffer ecb;
    public Entity prefab;
    public float deltaTime;

    private void Execute(ref CannonballSpawnPoint spawner, in Translation translation, in Rotation rotation)
    {
        // Entity cannonball = ecb.Instantiate(prefab);
        // float3 vecForward = new float3(0, 0, 1);
        //
        // //basically if it's lower then 0 you can wait then spawn
        // spawner.secondsToNextSpawn -= deltaTime;
        // if (spawner.secondsToNextSpawn >= 0) return;
        // spawner.secondsToNextSpawn += spawner.secondsBetweenSpawns;
        //
        // var direction = math.mul(rotation.Value, new float3(0f, 1f, 0f));
        //
        // ecb.SetComponent(cannonball, new Translation
        // {
        //     // Value = translation.Value + vecForward * spawner.maxDistanceFromSpawner,
        //     Value = translation.Value 
        // });
        
    }
}