using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(RaycastInputSystem))]
public partial class PlayerBounceSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem ecbSystem;
    
    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        RequireSingletonForUpdate<Player>();
    }

    protected override void OnUpdate()
    {
        var playerBounceJob = new PlayerBounceJob
        {
            
        };
        var handle = playerBounceJob.Schedule();
        ecbSystem.AddJobHandleForProducer(handle);
        handle.Complete();
    }
}

/**
 * Reads data from player's parabola-component and updates player translation
 */
[BurstCompile]
public partial struct PlayerBounceJob : IJobEntity
{
    public void Execute(in Player player, in ParabolaComp parabola, ref Translation translation)
    {
        if (parabola.t <= 1f)
        {
            float y = parabola.a * parabola.t * parabola.t + 
                      parabola.b * parabola.t + parabola.c;
            
            float x = math.lerp(player.currentX, player.targetX, parabola.t);
            float z = math.lerp(player.currentY, player.targetY, parabola.t);   
            
            translation.Value = new float3(x, y, z);
        }
    }
}