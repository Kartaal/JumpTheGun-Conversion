using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(PlayerBounceSystem))]
public partial class CannonballBounceSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        RequireSingletonForUpdate<CannonballData>();
    }

    protected override void OnUpdate()
    {
        var player = GetSingleton<Player>();
        
        var playerBounceJob = new CannonballBounceJob
        {
            playerTargetX = player.targetX,
            playerTargetY = player.targetY,
        };
        
        var handle = playerBounceJob.Schedule();
        ecbSystem.AddJobHandleForProducer(handle);
    }
}

/**
 * Reads data from player's parabola-component and updates player translation
 */
//[BurstCompile]
//[WithAll(typeof(Player))]
public partial struct CannonballBounceJob : IJobEntity
{
    public float playerTargetX;
    public float playerTargetY;

    public void Execute(ref CannonballData cannonball, in ParabolaComp parabola, ref Translation translation)
    {
        // Debug.Log(cannonball.currentX +" cannon"+cannonball.currentY +" parabola");
        if (parabola.t <= 1f)
        {
            float y = parabola.a * parabola.t * parabola.t + parabola.b * parabola.t + parabola.c;

            float x = math.lerp(cannonball.currentX, playerTargetX, parabola.t);
            float z = math.lerp(cannonball.currentY, playerTargetY, parabola.t);

            translation.Value = new float3(x, y, z);
        }
    }
}