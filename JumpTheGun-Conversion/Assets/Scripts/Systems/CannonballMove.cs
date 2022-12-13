using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(CannonballSpawningSystem))]
public partial class CannonballMove : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        
        float deltaTime = Time.DeltaTime;
        var cannonballSpawnJob = new CannonballMoveJob
        {
            ecb = ecbSystem.CreateCommandBuffer(),
            dt = deltaTime
        };

        var handle = cannonballSpawnJob.Schedule();
        ecbSystem.AddJobHandleForProducer(handle);
    }
}

public partial struct CannonballMoveJob : IJobEntity
{
    public EntityCommandBuffer ecb;
    public float dt;

    private void Execute(ref Translation trans, ref Rotation rot, ref CannonballData spawner)
    {
        trans.Value += spawner.speed * math.forward(rot.Value) * dt;
    }
}