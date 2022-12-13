using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public partial class CleanUpFiringSystem : SystemBase
{
    //convert to job
    protected override void OnUpdate()
    {
        float dt = Time.DeltaTime;

        Entities.ForEach((ref CannonballData spawner) =>
        {
            if (spawner.timeLeft <= 0)
                EntityManager.DestroyEntity(spawner.entity);

            spawner.timeLeft -= dt;
        }).WithStructuralChanges().Run();
    }
}