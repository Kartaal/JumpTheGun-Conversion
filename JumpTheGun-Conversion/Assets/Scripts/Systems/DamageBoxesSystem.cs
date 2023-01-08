using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(CannonballBounceSystem))]
public partial class DamageBoxesSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem ecbSystem;
    private StepPhysicsWorld stepPhysicsWorld;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        var ecb = ecbSystem.CreateCommandBuffer();
        var gameData = GetSingleton<GameData>();

        var dmgColl = new DamageCollisionJob
        {
            fixedDMG = gameData.boxHeightDamage,
            allAffectedTag = GetComponentDataFromEntity<TriggerAffectedTag>(true),
            damageable = GetComponentDataFromEntity<DamageableTag>(true),
            currentHp = GetComponentDataFromEntity<Health>(true),
            player = GetComponentDataFromEntity<Player>(true),
            boxComp = GetComponentDataFromEntity<DamageableTag>(true),
            cannonball = GetComponentDataFromEntity<CannonballData>(true),
            ecb = ecb
        }.Schedule(stepPhysicsWorld.Simulation, Dependency);

        var job = new AdjustHeightAfterDMGJob
        {
            minHeight = gameData.minHeight,
        };
        var handle = job.Schedule(dmgColl);
        
        ecbSystem.AddJobHandleForProducer(handle);
        Dependency = handle;
        handle.Complete();
    }
}

[BurstCompile]
public partial struct DamageCollisionJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentDataFromEntity<TriggerAffectedTag> allAffectedTag;
    [ReadOnly] public ComponentDataFromEntity<DamageableTag> damageable;
    [ReadOnly] public ComponentDataFromEntity<Health> currentHp;
    [ReadOnly] public ComponentDataFromEntity<DamageableTag> boxComp;
    [ReadOnly] public ComponentDataFromEntity<Player> player;
    [ReadOnly] public ComponentDataFromEntity<CannonballData> cannonball;

    public EntityCommandBuffer ecb;
    [ReadOnly] public float fixedDMG;

    public void Execute(TriggerEvent triggerEvent)
    {
        Entity entityA = triggerEvent.EntityA;
        Entity entityB = triggerEvent.EntityB;

        if (player.HasComponent(entityA) && cannonball.HasComponent(entityB))
        {
            //ecb.SetComponent(entityA, player);
            var playerData = player[entityA];
            playerData.isDead = true;
            ecb.SetComponent(entityA, playerData);
            //Debug.Log("PLAYER SUCCESSFULLY KILLED AS entityA");
        } 
        else if (player.HasComponent(entityB) && cannonball.HasComponent(entityA))
        {
            var playerData = player[entityB];
            playerData.isDead = true;
            ecb.SetComponent(entityB, playerData);
            //Debug.Log("PLAYER SUCCESSFULLY KILLED AS entityB");
        }
        
        if (boxComp.HasComponent(entityA) && cannonball.HasComponent(entityB))
        {
            ecb.SetComponent(entityA, new Health
            {
                //taking dmg from the same object (damaging obj)
                Value = currentHp[entityA].Value - fixedDMG
            });
            ecb.DestroyEntity(entityB);
        } 
        else if (boxComp.HasComponent(entityB) && cannonball.HasComponent(entityA))
        {
            ecb.SetComponent(entityB, new Health
            {
                //taking dmg from the same object (damaging obj)
                Value = currentHp[entityB].Value - fixedDMG
            });
            ecb.DestroyEntity(entityA);
        }
        
        
        // if (damageable.HasComponent(entityA) && allAffectedTag.HasComponent(entityB))
        // {
        //     if (currentHp.HasComponent(entityA))
        //     {
        //         ecb.SetComponent(entityA, new Health
        //         {
        //             //taking dmg from the same object (damaging obj)
        //             Value = currentHp[entityA].Value - fixedDMG
        //         });
        //         ecb.DestroyEntity(entityB);
        //     }
        //     
        //     
        // }
        // else if (damageable.HasComponent(entityB) && allAffectedTag.HasComponent(entityA))
        // {
        //     if (currentHp.HasComponent(entityB))
        //     {
        //         ecb.SetComponent(entityB, new Health
        //         {
        //             //taking dmg from the same object (damaging obj)
        //             Value = currentHp[entityB].Value - fixedDMG
        //         });
        //         ecb.DestroyEntity(entityA);
        //     }
        // }
    }
}

[BurstCompile]
public partial struct AdjustHeightAfterDMGJob : IJobEntity
{
    [ReadOnly] private float height;
    [ReadOnly] public float minHeight;

    private void Execute(ref Health health, ref Translation translation, ref NonUniformScale scale)
    {
        //need to figure out why it's not the default size
        if (health.Value <= minHeight)
        {
            health.Value = minHeight;
        }
        
        scale.Value.y = health.Value;
        translation.Value.y = health.Value / 2f;
    }
}