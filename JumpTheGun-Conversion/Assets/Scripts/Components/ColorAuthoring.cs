using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class ColorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public Color minColor;
    public Color maxColor;
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new HeightColors
        {
            maxColor = new float4(maxColor.r, maxColor.g, maxColor.b, maxColor.a),
            minColor = new float4(minColor.r, minColor.g, minColor.b, minColor.a)
        });
    }
}

public struct HeightColors : IComponentData
{
    public float4 minColor;
    public float4 maxColor;
}