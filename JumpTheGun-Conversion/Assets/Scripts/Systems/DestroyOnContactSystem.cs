using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

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
    }
}


[BurstCompile]
public partial struct DestroyPlayerOnLowHealthJob : IJobEntity
{
    public EntityCommandBuffer ecb;
    //[ReadOnly] public ComponentDataFromEntity<Player> player;

    private void Execute(in Player player)
    {

        if (player.isDead)
        {
            // Invoke reload sequence
        }
        
        /*
        if (health.Value > 0) return;
        
        if (player.HasComponent(entity))
        {
            var playerData = player[entity];
            ecb.SetComponent(entity, new Player
            {
                isDead = true,
                targetX = playerData.targetX,
                targetY = playerData.targetY,
            });
        }
        else
            ecb.DestroyEntity(entity);
        */
    }
}