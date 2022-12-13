using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
        //float a = 0f;
        //float b = 0f;
        //float c = 0f;
        //Parabola.Create(0f, 0f, 0f, out a, out b, out c);

        var playerBounceJob = new PlayerBounceJob
        {
            
        };
        var handle = playerBounceJob.Schedule();
        ecbSystem.AddJobHandleForProducer(handle);
    }
}

//[BurstCompile]
//[WithAll(typeof(Player))] //playerprefab instead?
public partial struct PlayerBounceJob : IJobEntity
{
    public void Execute(ref Player player, ref ParabolaComp parabola, ref Translation translation)
    {
        //Debug.Log($"parabola time is {parabola.t}");
        if (parabola.t <= 1f)
        {
            float y = Parabola.Solve(parabola.a, parabola.b, parabola.c, parabola.t);

            float x = math.lerp(translation.Value.x, player.targetX, parabola.t);
            float z = math.lerp(translation.Value.z, player.targetY, parabola.t);

            translation.Value = new float3(x, y, z);
        }
    }
}