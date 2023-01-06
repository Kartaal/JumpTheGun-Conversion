using Unity.Entities;

[GenerateAuthoringComponent]
public struct CannonballData : IComponentData
{
    public Entity entity;
    public float timeLeft;
    public float speed;
    
    public int startX;
    public int startY;
    public int targetX;
    public int targetY;
}