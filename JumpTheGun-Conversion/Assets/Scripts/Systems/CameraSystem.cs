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
        var updateCamJob = new UpdateCameraJob();
        var handle = updateCamJob.Schedule();
        ecbSystem.AddJobHandleForProducer(handle);
        handle.Complete();

    }
    
}


[WithAll(typeof(Player))]
public partial struct UpdateCameraJob : IJobEntity
{
    public void Execute(in Translation translation)
    {
        MonoCameraController.instance.UpdateCamTarget(translation.Value);
    }
}
