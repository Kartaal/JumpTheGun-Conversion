using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = UnityEngine.Random;

public partial class SpawnTerrainSystem : SystemBase
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
        
        float randomSeed = Random.Range(Int32.MinValue, Int32.MaxValue);

        hasRun = true;
        var job = new SpawnBoxJob
        {
            ecb = ecb,
            randomSeed = randomSeed
        };

        var handle = job.Schedule();
        ecbSystem.AddJobHandleForProducer(handle);
    }
}

[BurstCompile]
// [WithAll(typeof(NewBoolComponent))] add this
public partial struct SpawnBoxJob : IJobEntity
{
    public EntityCommandBuffer ecb;

    private float height; 
    // public int width, length;

    // Required because Random.Range doesn't work outside main thread
    private Unity.Mathematics.Random random;
    public float randomSeed;

    private void Execute(in BoxPrefabComp prefab, ref GameData gameData)
    {
        random = new Unity.Mathematics.Random((uint)randomSeed);

        float col = gameData.width;
        float row = gameData.height;

        gameData.boxes = ecb.AddBuffer<BoxesComponent>(gameData.manager);
        
        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < row; j++)
            {
                Entity entity = ecb.Instantiate(prefab.Value);

                // Adding to the DynamicBuffer
                gameData.boxes.Add(new BoxesComponent
                {
                    entity = entity
                });

                // height scaling
                height = random.NextFloat(gameData.minHeight, gameData.maxHeight);
                ecb.AddComponent(entity, new NonUniformScale
                {
                    Value = new float3(1, height, 1)
                });

                ecb.SetComponent(entity, new Translation
                {
                    // Height / 2 to ensure bottom of box aligns with y = 0
                    Value = new float3(i, height / 2f, j)
                });
            }
        }
    }

}
