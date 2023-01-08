using Unity.Burst;
using Unity.Collections;
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
        float dt = Time.DeltaTime;
        
        var cannonballBounceJob = new CannonballBounceJob
        {
            dt = dt
        };

        var handle = cannonballBounceJob.Schedule();
        ecbSystem.AddJobHandleForProducer(handle);
        handle.Complete();

    }
}

/**
 * Reads data from cannonball's parabola-component and updates cannonball translation
 */
[BurstCompile]
public partial struct CannonballBounceJob : IJobEntity
{
    [ReadOnly] public float dt;
    
    public void Execute(in CannonballData cannonball, ref ParabolaComp parabola, ref Translation translation)
    {
        parabola.t += dt;
        float simT = parabola.t / cannonball.duration;
        
        if (simT <= cannonball.duration)
        {
            float y = parabola.a * simT * simT + parabola.b * simT + parabola.c;
            
            float x = Mathf.LerpUnclamped(cannonball.startX, cannonball.targetX, simT);
            float z = Mathf.LerpUnclamped(cannonball.startY, cannonball.targetY, simT);

            translation.Value = new float3(x, y, z);
        }
    }
}