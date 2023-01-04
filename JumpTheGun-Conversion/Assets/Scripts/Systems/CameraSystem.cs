using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial class CameraSystem : SystemBase
{

    private BeginSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        RequireSingletonForUpdate<Player>();
    }

    protected override void OnUpdate()
    {
        //new UpdateCameraJob().Run();

        var updateCamJob = new UpdateCameraJob();
        var handle = updateCamJob.Schedule();
        ecbSystem.AddJobHandleForProducer(handle);
    }
    
}


[WithAll(typeof(Player))]
public partial struct UpdateCameraJob : IJobEntity
{
    public void Execute(in Translation translation)
    {
        MonoCameraController.Instance.UpdateTargetPosition(translation.Value);
    }
}
