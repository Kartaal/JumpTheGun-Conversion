using Unity.Entities;


[GenerateAuthoringComponent]
public struct GameData : IComponentData
{
    public Entity manager;
    
    // Terrain comp
    public float minHeight;
    public float maxHeight;

    public float height;
    public int width;

    public float boxHeightDamage;

    public int tankCount;
    // public bool gameOver;
}