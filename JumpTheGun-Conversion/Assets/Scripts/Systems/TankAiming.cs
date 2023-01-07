using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(SpawnTerrainSystem))]
public partial class TankAiming : SystemBase
{
    public BeginSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        EntityQuery query = GetEntityQuery(typeof(Tank));
        RequireForUpdate(query);
    }

    protected override void OnUpdate()
    {
        var player = GetSingletonEntity<Player>();
        var playerPos = GetComponent<Translation>(player);

        var aimJob = new TankAimJob
        {
            playerPos = playerPos
        };

        var jobHandle = aimJob.Schedule();
        
        ecbSystem.AddJobHandleForProducer(jobHandle);

    }
}

[BurstCompile]
[WithAll(typeof(Tank))]
public partial struct TankAimJob : IJobEntity
{
    [ReadOnly] public Translation playerPos;

    public void Execute(in Translation translation, ref Rotation rotation)
    {
        float3 diff = playerPos.Value - translation.Value;
        float angle = math.atan2(diff.x, diff.z);
        rotation.Value = quaternion.EulerXYZ(0, angle, 0);
    }
}