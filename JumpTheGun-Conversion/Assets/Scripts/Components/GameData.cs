using Unity.Entities;


[GenerateAuthoringComponent]
public struct GameData : IComponentData
{
    // Terrain comp
    public float minHeight;
    public float maxHeight;

    public int length;
    public int width;
}