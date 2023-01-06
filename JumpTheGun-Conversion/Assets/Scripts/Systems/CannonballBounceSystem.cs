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
        };

        var handle = playerBounceJob.Schedule();
        ecbSystem.AddJobHandleForProducer(handle);
    }
}

/**
 * Reads data from cannonball's parabola-component and updates cannonball translation
 */
public partial struct CannonballBounceJob : IJobEntity
{
    public void Execute(in CannonballData cannonball, in ParabolaComp parabola, ref Translation translation)
    {
        if (parabola.t <= 1f)
        {
            float y = parabola.a * parabola.t * parabola.t + parabola.b * parabola.t + parabola.c;

            float x = math.lerp(cannonball.startX, cannonball.targetX, parabola.t);
            float z = math.lerp(cannonball.startY, cannonball.targetY, parabola.t);

            translation.Value = new float3(x, y, z);
        }
    }
}