using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
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

    private void Execute(in BoxPrefabComp prefab, in PlayerPrefab playerPrefab, in TankPrefab tankPrefab, ref GameData gameData)
    {
        random = new Unity.Mathematics.Random((uint)randomSeed);

        NativeArray<float> heights = new NativeArray<float>(100, Allocator.Temp);
        NativeArray<int> occupiedIndices = new NativeArray<int>(6, Allocator.Temp);

        float col = gameData.width;
        float row = gameData.height;

        gameData.boxes = ecb.AddBuffer<BoxesComponent>(gameData.manager);
        
        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < row; j++)
            {
                Entity boxEntity = ecb.Instantiate(prefab.Value);

                // Adding to the DynamicBuffer
                gameData.boxes.Add(new BoxesComponent
                {
                    entity = boxEntity,
                    occupied = false
                });

                // height scaling
                height = random.NextFloat(gameData.minHeight, gameData.maxHeight);
                ecb.AddComponent(boxEntity, new NonUniformScale
                {
                    Value = new float3(1, height, 1)
                });

                ecb.SetComponent(boxEntity, new Translation
                {
                    // Height / 2 to ensure bottom of box aligns with y = 0
                    Value = new float3(i, height / 2f, j)
                });

                heights[(int)(col * i + j)] = height;
            }
        }
        
        

        Entity playerEntity = ecb.Instantiate(playerPrefab.entity);

        int playerX = random.NextInt(0, (int)col);
        int playerY = random.NextInt(0, (int)row);
        float playerHeight = heights[(int)col * playerX + playerY] + 0.3f;
        // gameData.boxes[(int)col * playerX + playerY].occupied = true; // Figure out how to do this
        occupiedIndices[0] = (int)col * playerX + playerY;

        ecb.SetComponent(playerEntity, new Translation
        {
            Value = new float3(playerX, playerHeight, playerY)
        });


        int tankX = -1;
        int tankY = -1;
        var tankIndex = -1;
        // Spawn tanks
        for (int count = 0; count < 5; count++)
        {
            Entity tankEntity = ecb.Instantiate(tankPrefab.entity);
            
            tankX = random.NextInt(0, (int)col);
            tankY = random.NextInt(0, (int)row);
            tankIndex = (int)col * tankX + tankY;

            

        }
    }

}
