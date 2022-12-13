using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial class CannonballMove : SystemBase
{
    protected override void OnUpdate()
    {
        float dt = Time.DeltaTime;


        Entities.ForEach((ref Translation trans, ref Rotation rot, ref CannonballData spawner) =>
        {
            trans.Value += spawner.speed * math.forward(rot.Value) * dt;
        }).WithStructuralChanges().Run();
    }
}