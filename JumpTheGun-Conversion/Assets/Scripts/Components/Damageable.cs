using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

public class Damageable : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private int startingHealth = 100;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<Damage>(entity);
        dstManager.AddComponentData(entity, new Health { Value = startingHealth});
    }
}