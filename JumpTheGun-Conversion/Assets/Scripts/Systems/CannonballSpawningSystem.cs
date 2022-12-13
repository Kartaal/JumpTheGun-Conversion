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
        float deltaTime = Time.DeltaTime;

        // if (UnityEngine.Time.frameCount % 240 == 0) // Shitty fire rate limitation for now...
        // {
        Entity cannonballPrefab = GetSingleton<CannonballPrefab>().entity;

        var cannonballSpawnJob = new CannonballSpawnJob
        {
            ecb = ecbSystem.CreateCommandBuffer(),
            prefab = cannonballPrefab,
            dt = deltaTime
        };

        cannonballSpawnJob.Run();

        // ecbSystem.AddJobHandleForProducer(handle);
        // }
    }
}


[BurstCompile]
// [WithAll(typeof(Tank))]
public partial struct CannonballSpawnJob : IJobEntity
{
    public EntityCommandBuffer ecb;
    public Entity prefab;
    public float dt;

    private void Execute(in Translation translation, ref CannonballSpawnPoint spawnPoint)
    {
        //Figure out why there is a delay before the first spawn
        //(unless this it totally correct behaviour, which it might be)
        spawnPoint.secondsBetweenSpawns += dt;
        if (spawnPoint.secondsBetweenSpawns > spawnPoint.secondsToNextSpawn)
        {
            Entity cannonball = ecb.Instantiate(prefab);
            spawnPoint.secondsBetweenSpawns = 0;

            ecb.SetComponent(cannonball, new Translation
            {
                Value = translation.Value
            });
        }
    }
}