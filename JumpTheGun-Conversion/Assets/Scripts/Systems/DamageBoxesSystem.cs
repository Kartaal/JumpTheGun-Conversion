using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

[UpdateAfter(typeof(ResolveDamageSystem))]
// [UpdateAfter(typeof(SetBoxHeight))]
public partial class DamageBoxesSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem ecbSystem;
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;
    private bool hasRun;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        // // Enabled=false or tags (use components if we need to restart)
        if (hasRun) return;
        var ecb = ecbSystem.CreateCommandBuffer();
        // float randomSeed = UnityEngine.Random.Range(Int32.MinValue, Int32.MaxValue);
        // hasRun = true;
        // var dealDamageBoxJob = new DealDamageBoxJob
        // {
        //     ecb = ecb,
        //     randomSeed = randomSeed
        // };
        //
        // var damageJobHandle = dealDamageBoxJob.Schedule();
        // ecbSystem.AddJobHandleForProducer(damageJobHandle);

        ////after taking dmg adjust height
        var job = new AdjustHeightAfterDMGJob
        {
            ecb = ecb,
        };
        var handle = job.Schedule();
        // job.Schedule(damageJobHandle).Complete();
        ecbSystem.AddJobHandleForProducer(handle);

        var dmgColl = new DamageCollisionJob
        {
            dealDamageGroup = GetComponentDataFromEntity<DealDamage>(true),
            allAffectedTag = GetComponentDataFromEntity<TriggerAffectedTag>(true),
            damageableBox = GetComponentDataFromEntity<DamageableTag>(true),
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
    [ReadOnly] public ComponentDataFromEntity<DamageableTag> damageableBox;
    [ReadOnly] public ComponentDataFromEntity<Health> currentHp;

    public EntityCommandBuffer ecb;

    public void Execute(TriggerEvent triggerEvent)
    {
        Entity entityA = triggerEvent.EntityA;
        Entity entityB = triggerEvent.EntityB;

        if (damageableBox.HasComponent(entityA) && allAffectedTag.HasComponent(entityB))
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
        else if (damageableBox.HasComponent(entityB) && allAffectedTag.HasComponent(entityA))
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
// [WithAll(typeof(NewBoolComponent))] add this
public partial struct DealDamageBoxJob : IJobEntity
{
    public EntityCommandBuffer ecb;

    private float height;

    // Required because Random.Range doesn't work outside main thread
    private Random random;
    public float randomSeed;

    private void Execute(Entity entity, ref GameData gameData)
    {
        random = new Random((uint)randomSeed);
        // height scaling
        height = random.NextFloat(gameData.minHeight, gameData.maxHeight);

        // float damagedHealth = health.Value - gameData.height;
        float damagedHealth = gameData.height = height;

        // Adding to the DynamicBuffer
        ecb.SetComponent(entity, new Health
        {
            Value = damagedHealth
        });

        ecb.AddComponent(entity, new NonUniformScale
        {
            Value = new float3(1, damagedHealth, 1)
        });
    }
}

[BurstCompile]
public partial struct AdjustHeightAfterDMGJob : IJobEntity
{
    public EntityCommandBuffer ecb;
    private float height;

    private void Execute(Entity entity, ref GameData gameData, ref Health health)
    {
        //need to figure out why it's not the default size
        if (health.Value <= gameData.minHeight)
        {
            health.Value = gameData.minHeight;
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