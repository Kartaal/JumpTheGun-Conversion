using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BoxPrefabComp : IComponentData
{
    public Entity Value;
}
