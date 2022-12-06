using Unity.Entities;


[GenerateAuthoringComponent]
public struct GameData : IComponentData
{
    public Entity manager;
    
    // Terrain comp
    public float minHeight;
    public float maxHeight;

    public int height;
    public int width;

    // public DynamicBuffer<BoxesComponent> boxes;
}