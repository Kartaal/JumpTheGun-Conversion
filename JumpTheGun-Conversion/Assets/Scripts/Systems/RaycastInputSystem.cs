using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

public partial class RaycastInputSystem : SystemBase
{

    public BeginSimulationEntityCommandBufferSystem ecbSystem;
    public BuildPhysicsWorld _physicsWorld;
    public CollisionWorld _collisionWorld;

    public Vector3 mouseWorldPos;
    
    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        _physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
        RaycastInput raycastInput = new RaycastInput();
        RaycastHit raycastHit;

        if (Camera.main != null)
        {
            _collisionWorld = _physicsWorld.PhysicsWorld.CollisionWorld;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var rayOrigin = ray.origin;
            var rayEnd = ray.GetPoint(100f);

            raycastInput = new RaycastInput
            {
                Start = rayOrigin,
                End = rayEnd,
                Filter = new CollisionFilter
                {
                    BelongsTo = (uint) 0xffffffff,
                    CollidesWith = (uint) PhysicsLayerEnum.Floor
                }
            };
            raycastHit = new RaycastHit();
        }

        if (_collisionWorld.CastRay(raycastInput, out raycastHit))
        {
            float3 hitPos = raycastHit.Position;
            
            // Schedule player direction job
        }
    }
}

[BurstCompile]
public partial struct PlayerDirectionJob : IJobEntity
{
    public float3 hitPos;

    //public void Execute(in Player player, DynamicBuffer<BoxComponent> buffer)
    //{
        // Find closest box coords in hitPos direction
        
        // Use box coords as index into buffer
    //    BoxComponent box = buffer[hitPos.x + hitPos.y * height];
    //    Entity boxEntity = box.entity;
        
        // Use calculations from original code for height value of parabola and the parabola parameters
    //    NonUniformScale boxScale = nonuniforms[boxEntity];

    //    boxTranslation.y; // use for height calculations of parabola, see original code in Player


        // Set parabola params...




    //}
}