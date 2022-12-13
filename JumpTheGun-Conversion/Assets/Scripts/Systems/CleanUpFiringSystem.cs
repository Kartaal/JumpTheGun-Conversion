using Unity.Burst;
using Unity.Entities;

[UpdateAfter(typeof(CannonballMove))]
[UpdateAfter(typeof(CannonballSpawningSystem))]
public partial class CleanUpFiringSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        var cannonballSpawnJob = new CannonCleanUpJob
        {
            ecb = ecbSystem.CreateCommandBuffer(),
            dt = deltaTime
        };

        var handle = cannonballSpawnJob.Schedule();
        ecbSystem.AddJobHandleForProducer(handle);
    }
}


[BurstCompile]
public partial struct CannonCleanUpJob : IJobEntity
{
    public EntityCommandBuffer ecb;
    public float dt;

    private void Execute(ref CannonballData spawner)
    {
        if (spawner.timeLeft <= 0)
            ecb.DestroyEntity(spawner.entity);

        spawner.timeLeft -= dt;
    }
}