// using Unity.Entities;
// using Unity.Transforms;
//
// public class SpawnerSystemForEach : ComponentSystem
// {
//     private float spawnTimer;
//
//     protected override void OnUpdate()
//     {
//         float dt = Time.DeltaTime;
//         Entities.ForEach((ref CannonballSpawnPoint spawner) =>
//         {
//             spawner.secondsToNextSpawn += dt;
//             if (spawner.secondsToNextSpawn > spawner.secondsBetweenSpawns)
//             {
//                 spawner.secondsToNextSpawn -= spawner.secondsBetweenSpawns;
//                 Entity entity = EntityManager.Instantiate(spawner.entity);
//                 EntityManager.SetComponentData(entity, EntityManager.GetComponentData<Rotation>(spawner.point));
//                 EntityManager.SetComponentData(entity,
//                     EntityManager.GetComponentData<Translation>(spawner.point));
//             }
//         });
//     }
// }