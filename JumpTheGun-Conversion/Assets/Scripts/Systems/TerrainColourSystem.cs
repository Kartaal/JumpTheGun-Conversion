using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

//[UpdateAfter(typeof(TerrainDamage))] // Ensure colour is updated according to new height!
public partial class TerrainColourSystem : SystemBase
{
    public BeginSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate()
    {
        var ecb = ecbSystem.CreateCommandBuffer();
        var gameData = GetSingleton<GameData>();

        var colorLerpJob = new ColorLerpJob
        {
            minHeight = gameData.minHeight,
            maxHeight = gameData.maxHeight
        };
        
        var handle = colorLerpJob.Schedule();
        ecbSystem.AddJobHandleForProducer(handle);
    }
}

[BurstCompile]
public partial struct ColorLerpJob : IJobEntity
{
    public float minHeight;
    public float maxHeight;

    public void Execute(ref URPMaterialPropertyBaseColor baseColor, in HeightColors heightColors, in NonUniformScale scale)
    {
        float heightPercent = scale.Value.y - minHeight;
        float height100Percent = maxHeight - minHeight;
        
        baseColor.Value = math.lerp(heightColors.minColor, heightColors.maxColor, heightPercent / height100Percent);
    }
}
