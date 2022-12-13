using Unity.Entities;
using UnityEngine.UIElements;

[GenerateAuthoringComponent]
public struct CannonballSpawnPoint : IComponentData
{
    public Entity entity;
    public Entity point;
    public float secondsBetweenSpawns;
    public float secondsToNextSpawn;
}
