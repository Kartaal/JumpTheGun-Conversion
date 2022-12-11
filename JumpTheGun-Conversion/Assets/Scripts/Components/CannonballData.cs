using Unity.Entities;

[GenerateAuthoringComponent]
public struct CannonballData : IComponentData
{
    public Entity entity;
    public float timeLeft;
}