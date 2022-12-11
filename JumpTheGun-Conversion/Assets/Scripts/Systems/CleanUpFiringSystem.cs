using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public partial class CleanUpFiringSystem : SystemBase
{
    protected override void OnUpdate()
    {
    }
    
    // public struct CleanUpFiringJob:IJobParallelFor
    // {
        // [ReadOnly] public EntityArray Entitites;
        // public EntityCommandBuffer ecb;
        // public float currentTime;
        // public ComponentDataArray<Firing> Firings;
        // public void Execute(int index)
        // {
        //     if(currentTime - Firings[index].FiredAt <0.5) return;
        //     ecb.RemoveComponent<Firing>(Entitites[index]);
        // }
    // }
    
}