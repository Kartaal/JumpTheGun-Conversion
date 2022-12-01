using Unity.Entities;

// Capacity because we only ever have 100 boxes - based on GameData height and width
[InternalBufferCapacity(100)]
public struct BoxesComponent : IBufferElementData
{
    public Entity entity;
}
