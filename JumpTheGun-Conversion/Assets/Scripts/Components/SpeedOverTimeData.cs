using Unity.Entities;

[GenerateAuthoringComponent]
public struct SpeedOverTimeData : IComponentData
{
    public double increasePerSecond;
}