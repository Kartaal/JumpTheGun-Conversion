using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Player : IComponentData
{
    public int targetX;
    public int targetY;
    public int currentX;
    public int currentY;
    public bool isTargetOccupied;
    public bool isDead;
}
