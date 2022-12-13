using Unity.Entities;

[GenerateAuthoringComponent]
public struct Tank : IComponentData
{
    public Entity entity;
    
    public float secondsBetweenSpawns;
    public float secondsToNextSpawn;
}
