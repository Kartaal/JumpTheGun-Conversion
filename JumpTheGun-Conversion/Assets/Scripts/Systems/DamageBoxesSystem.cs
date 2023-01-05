using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

public partial class DamageBoxesSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem ecbSystem;
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        var ecb = ecbSystem.CreateCommandBuffer();
        var gameData = GetSingleton<GameData>();

        var job = new AdjustHeightAfterDMGJob
        {
            ecb = ecb,
            minHeight = gameData.minHeight,
        };
        var handle = job.Schedule();
        ecbSystem.AddJobHandleForProducer(handle);

        var dmgColl = new DamageCollisionJob
        {
            dealDamageGroup = GetComponentDataFromEntity<DealDamage>(true),
            allAffectedTag = GetComponentDataFromEntity<TriggerAffectedTag>(true),
            damageable = GetComponentDataFromEntity<DamageableTag>(true),
            currentHp = GetComponentDataFromEntity<Health>(true),
            ecb = ecb
        }.Schedule(stepPhysicsWorld.Simulation, Dependency);

        dmgColl.Complete();
        // ecb.Playback(EntityManager);
    }
}

[BurstCompile]
public partial struct DamageCollisionJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentDataFromEntity<DealDamage> dealDamageGroup;

    // [ReadOnly] public ComponentDataFromEntity<TriggerApplyScaleTag> allApplyScaleTag;
    [ReadOnly] public ComponentDataFromEntity<TriggerAffectedTag> allAffectedTag;
    [ReadOnly] public ComponentDataFromEntity<DamageableTag> damageable;
    [ReadOnly] public ComponentDataFromEntity<Health> currentHp;

    public EntityCommandBuffer ecb;

    public void Execute(TriggerEvent triggerEvent)
    {
        Entity entityA = triggerEvent.EntityA;
        Entity entityB = triggerEvent.EntityB;

        if (damageable.HasComponent(entityA) && allAffectedTag.HasComponent(entityB))
        {
            if (currentHp.HasComponent(entityA))
            {
                ecb.SetComponent(entityA, new Health
                {
                    //taking dmg from the same object (damaging obj)
                    Value = currentHp[entityA].Value - dealDamageGroup[entityB].Value
                });
                ecb.DestroyEntity(entityB);
            }
        }
        else if (damageable.HasComponent(entityB) && allAffectedTag.HasComponent(entityA))
        {
            if (currentHp.HasComponent(entityB))
            {
                ecb.SetComponent(entityB, new Health
                {
                    //taking dmg from the same object (damaging obj)
                    Value = currentHp[entityB].Value - dealDamageGroup[entityA].Value
                });
                ecb.DestroyEntity(entityA);
            }
        }
    }
}

[BurstCompile]
public partial struct AdjustHeightAfterDMGJob : IJobEntity
{
    public EntityCommandBuffer ecb;
    private float height;
    public float minHeight;

    private void Execute(Entity entity, ref Health health)
    {
        //need to figure out why it's not the default size
        if (health.Value <= minHeight)
        {
            health.Value = minHeight;
            return;
        }

        // Adding to the DynamicBuffer
        ecb.SetComponent(entity, new Health
        {
            Value = health.Value
        });

        ecb.AddComponent(entity, new NonUniformScale
        {
            Value = new float3(1, health.Value, 1)
        });
    }
}