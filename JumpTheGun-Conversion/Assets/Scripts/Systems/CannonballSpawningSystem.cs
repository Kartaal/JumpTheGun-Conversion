using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(TankAiming))]
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

        var gameData = GetSingleton<GameData>();
        var nonUniforms = GetComponentDataFromEntity<NonUniformScale>(true);

        Entity cannonballPrefab = GetSingleton<CannonballPrefab>().entity;
        var player = GetSingleton<Player>();

        var cannonballSpawnJob = new CannonballSpawnJob
        {
            ecb = ecbSystem.CreateCommandBuffer(),
            prefab = cannonballPrefab,
            playerPosX = player.currentX,
            playerPosY = player.currentY,
            dt = deltaTime,
            boxes = GetBuffer<BoxesComponent>(gameData.manager),
            nonUniforms = nonUniforms,
        };

        var handle = cannonballSpawnJob.Schedule();
        ecbSystem.AddJobHandleForProducer(handle);
    }
}


[BurstCompile]
public partial struct CannonballSpawnJob : IJobEntity
{
    public EntityCommandBuffer ecb;
    [ReadOnly] public Entity prefab;
    [ReadOnly] public float dt;
    [ReadOnly] public int playerPosX;
    [ReadOnly] public int playerPosY;

    public int col;

    // public int row;
    [ReadOnly] public ComponentDataFromEntity<NonUniformScale> nonUniforms;
    [ReadOnly] public DynamicBuffer<BoxesComponent> boxes;

    private Random random;

    private void Execute(in Translation translation, ref Tank spawnPoint)
    {
        //Figure out why there is a delay before the first spawn
        //(unless this it totally correct behaviour, which it might be)
        spawnPoint.secondsBetweenSpawns += dt;
        if (spawnPoint.secondsBetweenSpawns > spawnPoint.secondsToNextSpawn)
        {
            Entity cannonball = ecb.Instantiate(prefab);
            spawnPoint.secondsBetweenSpawns = 0;
            //spawnPoint.secondsToNextSpawn = UnityEngine.Random.Range(spawnPoint.secondsToNextSpawn-1f
            //    , spawnPoint.secondsToNextSpawn+1f);
            random = new Random((uint)spawnPoint.entity.Index);
            var r = random.NextFloat(3f, 6f);
            spawnPoint.secondsToNextSpawn = r;

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
                targetX = playerPosX,
                targetY = playerPosY,
            });

            int currentBoxIndex = col * tankGridX + tankGridY; // from
            int targetBoxIndex = col * playerPosX + playerPosY; // to

            NonUniformScale currentBoxScale = nonUniforms[boxes[currentBoxIndex].entity];
            float startY = currentBoxScale.Value.y;

            NonUniformScale targetBoxScale = nonUniforms[boxes[targetBoxIndex].entity];
            float endY = targetBoxScale.Value.y;

            float height = math.max(startY, endY);
            height += 5f;

            float c = startY;

            float k = math.sqrt(math.abs(startY - height)) /
                      (math.sqrt(math.abs(startY - height)) +
                       math.sqrt(math.abs(endY - height)));

            float a = (height - startY - k * (endY - startY)) / (k * k - k);
            float b = endY - startY - a;
            float t = 0f; // reset t to start new parabola movement

            ecb.SetComponent(cannonball, new ParabolaComp
            {
                c = c,
                a = a,
                b = b,
                t = t
            });
        }
    }
}