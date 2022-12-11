using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

//https://www.youtube.com/watch?v=a9AUXNFBWt4
[AlwaysSynchronizeSystem]
public partial class IncreaseVelocityOverTimeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // float deltaTime = Time.DeltaTime;
        //
        // Entities.ForEach((ref PhysicsVelocity vel, in SpeedOverTimeData data) =>
        // {
        //     float3 modifier = new float3(data.increasePerSecond * deltaTime);
        //
        //     float3 newVel = vel.Linear.xyz;
        //
        //     newVel += math.lerp(-modifier, modifier, math.sign(newVel));
        //     vel.Linear.xyz = newVel;
        // }).Run();
    }
}