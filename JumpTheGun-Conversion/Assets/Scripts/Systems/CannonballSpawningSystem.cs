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
        RequireSingletonForUpdate<Player>();
    }

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entity cannonballPrefab = GetSingleton<CannonballPrefab>().entity;
        var player = GetSingleton<Player>();

        var cannonballSpawnJob = new CannonballSpawnJob
        {
            ecb = ecbSystem.CreateCommandBuffer(),
            prefab = cannonballPrefab,
            playerTargetX = player.targetX,
            playerTargetY = player.targetY,
            dt = deltaTime
        };

        var handle = cannonballSpawnJob.Schedule();
        ecbSystem.AddJobHandleForProducer(handle);
    }
}


[BurstCompile]
public partial struct CannonballSpawnJob : IJobEntity
{
    public EntityCommandBuffer ecb;
    public Entity prefab;
    public float dt;
    public int playerTargetX;
    public int playerTargetY;

    private void Execute(in Translation translation, ref Tank spawnPoint)
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

            int tankGridX = (int)math.round(translation.Value.x);
            int tankGridY = (int)math.round(translation.Value.z);
            ecb.SetComponent(cannonball, new CannonballData
            {
                entity = cannonball,
                timeLeft = 5,
                speed = 5,
                startX = tankGridX,
                startY = tankGridY,
                targetX = playerTargetX,
                targetY = playerTargetY,
            });
            
            
        }
    }
}