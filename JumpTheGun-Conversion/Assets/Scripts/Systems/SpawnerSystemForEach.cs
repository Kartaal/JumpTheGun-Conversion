using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//
// https://forum.unity.com/threads/questions-about-an-ecs-spawner-workflows.838195/
//
// https://www.google.com/search?q=after+spawning+it+will+move+forward+unity+ecs&oq=after+spawning+it+will+move+forward&aqs=chrome.2.69i57j33i160l2.5175j0j7&sourceid=chrome&ie=UTF-8
//
// https://www.google.com/search?q=destroy+object+after+time+unity+dots+job&sxsrf=ALiCzsaGc0cCDN1NI0NjxwsZ9sD6hWvoIA%3A1670781408376&ei=4BmWY5W3FqHl7_UP4qq-sAo&ved=0ahUKEwiVp9_ikfL7AhWh8rsIHWKVD6YQ4dUDCA8&uact=5&oq=destroy+object+after+time+unity+dots+job&gs_lcp=Cgxnd3Mtd2l6LXNlcnAQAzIFCCEQoAEyBQghEKABOgoIABBHENYEELADOgcIIRCgARAKSgQIQRgASgQIRhgAUKwBWL4GYLMIaAFwAXgAgAGKAYgBwwOSAQMxLjOYAQCgAQHIAQjAAQE&sclient=gws-wiz-serp
//
// https://forum.unity.com/threads/how-to-safely-destroy-entities.1283933/
//
// https://forum.unity.com/threads/set-a-world-transform-forward-for-pure-ecs-entity.899927/
//
// https://www.youtube.com/watch?v=NmqpzyeI6ZM


public class SpawnerSystemForEach : ComponentSystem
{
    private float spawnTimer;

    private EntityManager manager;
    private BeginSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        float dt = Time.DeltaTime;

        Entities.ForEach((ref CannonballSpawnPoint spawner, ref LocalToWorld localToWorld, ref Rotation rot) =>
        {
            spawner.secondsToNextSpawn += dt;
            if (spawner.secondsToNextSpawn > spawner.secondsBetweenSpawns)
            {
                spawner.secondsToNextSpawn -= spawner.secondsBetweenSpawns;

                Entity entity = EntityManager.Instantiate(spawner.entity);
                manager.SetComponentData(entity, EntityManager.GetComponentData<Rotation>(spawner.point));
                Translation translation = EntityManager.GetComponentData<Translation>(spawner.point);
                manager.SetComponentData(entity, translation);

                // var x = 1;
                // var y = 2;
                // var position = math.transform(localToWorld.Value,
                //     new float3(x * 2, 0, y * 2));
                // // translation.Value.z +=  movement.direction * movement.speed * dt;
                // // translation.Value.z += spawner.maxDistanceFromSpawner * dt;
                // translation.Value += position;
                // manager.SetComponentData(entity, translation);
                //
                // float ballSpeed = 300;
                //
                // translation.Value += ballSpeed * Time.DeltaTime * math.forward(rot.Value);
                // //
                // // float ballSpeed = 300;
                // // float3 dir = float3.zero + translation.Value;
                // // float3 speed = dir * ballSpeed;
                // // PhysicsVelocity velocity = new PhysicsVelocity
                // // {
                // //     Linear = speed,
                // //     Angular = float3.zero
                // // };
                // //
                // // // localToWorld.Value = spawner.maxDistanceFromSpawner * dt;
                // // manager.AddComponentData(entity, velocity);
            }
        });

        Entities.ForEach((ref Translation trans, ref Rotation rot, ref CannonballSpawnPoint spawner) =>
        {
            trans.Value += spawner.maxDistanceFromSpawner * Time.DeltaTime * math.forward(rot.Value) * dt;
        });
        
        Entities.ForEach((ref CannonballData spawner) =>
        {
            Debug.Log(spawner.timeLeft);
            if (spawner.timeLeft <= 0)
                EntityManager.DestroyEntity(spawner.entity);

            spawner.timeLeft -= dt;
        });
    }
}