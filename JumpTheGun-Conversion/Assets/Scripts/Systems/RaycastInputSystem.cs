using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

[UpdateAfter(typeof(SpawnTerrainSystem))]
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
        RequireSingletonForUpdate<Player>();
    }

    protected override void OnUpdate()
    {
        // Debugging things...
        if(Input.GetKey(KeyCode.A))
            Debug.Break();
        
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
            var gameData = GetSingleton<GameData>();
            var nonuniforms = GetComponentDataFromEntity<NonUniformScale>();

            // Schedule player direction job
            var playerDirJob = new PlayerDirectionJob
            {
                hitPos = hitPos,
                col = gameData.width,
                row = gameData.height,
                boxes = GetBuffer<BoxesComponent>(gameData.manager),
                nonuniforms = nonuniforms,
                frames = UnityEngine.Time.frameCount
            };

            var handle = playerDirJob.Schedule();
            ecbSystem.AddJobHandleForProducer(handle);
        }
    }
}

[BurstCompile]
public partial struct PlayerDirectionJob : IJobEntity
{
    public float3 hitPos;
    public int col;
    public int row;
    public ComponentDataFromEntity<NonUniformScale> nonuniforms;
    public DynamicBuffer<BoxesComponent> boxes;
    public int frames;

    public void Execute(in Player player)
    {   
        // Find closest box coords in hitPos direction
        int gridX = (int)math.round(hitPos.x);
        int gridY = (int)math.round(hitPos.z);
        int boxIndex = col * gridX + gridY;

        if (boxIndex >= boxes.Length) return; // Just testing, ensuring that we don't attempt a non-working index
        
        // Use box coords as index into buffer
        BoxesComponent box = boxes[boxIndex];
        Entity boxEntity = box.entity;

        // Use calculations from original code for height value of parabola and the parabola parameters
        NonUniformScale boxScale = nonuniforms[boxEntity];

        //    boxTranslation.y; // use for height calculations of parabola, see original code in Player


        // Set parabola params...




        if (frames % 60 == 0)
        {
            // Debug.Log($"Hitpos: [{hitPos.x}, {hitPos.y}, {hitPos.z}]");
            // Debug.Log($"GridX: {gridX}");
            // Debug.Log($"GridY: {gridY}");
            Debug.Log($"BoxIndex: {boxIndex}");
            Debug.Log($"Box entity index: {boxEntity.Index}");
            Debug.Log($"Non-Uniform Scale: {boxScale.Value}");
        }

    }
}