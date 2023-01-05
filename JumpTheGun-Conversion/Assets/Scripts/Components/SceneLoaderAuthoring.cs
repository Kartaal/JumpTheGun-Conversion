using Unity.Entities;
using UnityEditor;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

public struct SceneLoader : IComponentData
{
    public Hash128 Guid;
}

#if UNITY_EDITOR
// Authoring component, a SceneAsset can only be used in the Editor
public class SceneLoaderAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public SceneAsset Scene;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var path = AssetDatabase.GetAssetPath(Scene);
        var guid = AssetDatabase.GUIDFromAssetPath(path);
        dstManager.AddComponentData(entity, new SceneLoader { Guid = guid });
    }
}
#endif