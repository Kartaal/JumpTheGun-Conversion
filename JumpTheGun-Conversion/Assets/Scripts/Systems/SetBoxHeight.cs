using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;


public partial class SetBoxHeight : SystemBase
{
    public EndInitializationEntityCommandBufferSystem ecbSystem;
    private bool hasRun = false;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        // Enabled=false or tags (use components if we need to restart)
        if (hasRun) return;
        
        var ecb = ecbSystem.CreateCommandBuffer();
        
        float randomSeed = UnityEngine.Random.Range(Int32.MinValue, Int32.MaxValue);
        
        hasRun = true;
        var job = new DealDamageBoxJob
        {
            ecb = ecb,
            randomSeed = randomSeed
        };
        
        var handle = job.Schedule();
        ecbSystem.AddJobHandleForProducer(handle);
    }
}

// [BurstCompile]
// // [WithAll(typeof(NewBoolComponent))] add this
// public partial struct DealDamageBoxJob : IJobEntity
// {
//     public EntityCommandBuffer ecb;
//
//     private float height;
//
//     // Required because Random.Range doesn't work outside main thread
//     private Random random;
//     public float randomSeed;
//
//     private void Execute(Entity entity, ref GameData gameData)
//     {
//         random = new Random((uint)randomSeed);
//         // height scaling
//         height = random.NextFloat(gameData.minHeight, gameData.maxHeight);
//
//         // float damagedHealth = health.Value - gameData.height;
//         float damagedHealth = gameData.height = height;
//
//         // Adding to the DynamicBuffer
//         ecb.SetComponent(entity, new Health
//         {
//             Value = damagedHealth
//         });
//
//         ecb.AddComponent(entity, new NonUniformScale
//         {
//             Value = new float3(1, damagedHealth, 1)
//         });
//     }
// }