using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[DisableAutoCreation]
[UpdateAfter(typeof(DamageBoxesSystem))]
public partial class DestroyOnContactSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = ecbSystem.CreateCommandBuffer();

        var aimJob = new DestroyPlayerOnLowHealthJob
        {
            ecb = ecb,
            //player = GetComponentDataFromEntity<Player>(true),
        };

        var jobHandle = aimJob.Schedule();

        ecbSystem.AddJobHandleForProducer(jobHandle);
        jobHandle.Complete();
    }
}


[BurstCompile]
public partial struct DestroyPlayerOnLowHealthJob : IJobEntity
{
    public EntityCommandBuffer ecb;
    //[ReadOnly] public ComponentDataFromEntity<Player> player;

    private void Execute(Entity entity, in Health health)
    {
        if (health.Value > 0) return;
        ecb.DestroyEntity(entity);
    }
}