using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(ResolveDamageSystem))]
public partial class DamageCollisionSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;
    private BeginSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        // var damageCollisionJob = new DamageCollisionJob
        // {
        //     damageGroup = GetBufferFromEntity<Damage>(),
        //     dealDamageGroup = GetComponentDataFromEntity<DealDamage>(true)
        // };
        // var handle = damageCollisionJob.Schedule(stepPhysicsWorld.Simulation , Dependency);
        // ecbSystem.AddJobHandleForProducer(handle);

        var ecb = new EntityCommandBuffer(World.UpdateAllocator.ToAllocator);
        var dmgColl = new DamageCollisionJob
        {
            damageGroup = GetBufferFromEntity<Damage>(),
            damageGroupColl = GetComponentDataFromEntity<DamageCollComp>(true),
            dealDamageGroup = GetComponentDataFromEntity<DealDamage>(true),
            // allApplyScaleTag = GetComponentDataFromEntity<TriggerApplyScaleTag>(true),
            allAffectedTag = GetComponentDataFromEntity<TriggerAffectedTag>(true),
            damageableBox = GetComponentDataFromEntity<DamageableTag>(true),
            currentHp = GetComponentDataFromEntity<Health>(true),
            playerColl = GetComponentDataFromEntity<Player>(true),
            ecb = ecb
        }.Schedule(stepPhysicsWorld.Simulation, Dependency);

        dmgColl.Complete();
        ecb.Playback(EntityManager);
    }
}

[BurstCompile]
public partial struct DamageCollisionJob : ITriggerEventsJob
{
    public BufferFromEntity<Damage> damageGroup;
    [ReadOnly] public ComponentDataFromEntity<DamageCollComp> damageGroupColl;
    [ReadOnly] public ComponentDataFromEntity<DealDamage> dealDamageGroup;

    // [ReadOnly] public ComponentDataFromEntity<TriggerApplyScaleTag> allApplyScaleTag;
    [ReadOnly] public ComponentDataFromEntity<TriggerAffectedTag> allAffectedTag;
    [ReadOnly] public ComponentDataFromEntity<DamageableTag> damageableBox;
    [ReadOnly] public ComponentDataFromEntity<Health> currentHp;
    [ReadOnly] public ComponentDataFromEntity<Player> playerColl;

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
                    // Value = currentHp-dealDamageGroup[entityB].Value
                    Value = currentHp[entityA].Value - dealDamageGroup[entityB].Value
                });
                
                ecb.DestroyEntity(entityB);
            }

            // if (playerColl.HasComponent(entityA))
            // {
                // ecb.SetComponent(entityA, new Player
                // {
                //     isDead = true
                // });
            // }

        }
        else if (damageableBox.HasComponent(entityB) && allAffectedTag.HasComponent(entityA))
        {
            ecb.AddComponent(entityA, new NonUniformScale
            {
                Value = new float3(3, 1, 1)
            });
        }
    }
}