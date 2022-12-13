using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public partial struct ParabolaComp : IComponentData
{
    public float BOUNCE_HEIGHT;
    public float a;
    public float b;
    public float c;
    public float t;
    
}
