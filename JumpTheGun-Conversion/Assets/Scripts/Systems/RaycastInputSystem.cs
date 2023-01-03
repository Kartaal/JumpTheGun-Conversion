using System;
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
        float dt = Time.DeltaTime;
        //var dt = Time.fixedDeltaTime;
        
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
            var rayEnd = ray.GetPoint(20f);

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
                frames = UnityEngine.Time.frameCount,
                dt = dt
            };

            var handle = playerDirJob.Schedule();
            ecbSystem.AddJobHandleForProducer(handle);
        }
    }
}

//[BurstCompile]
//[WithAll(typeof(Player))]
public partial struct PlayerDirectionJob : IJobEntity
{
    public float3 hitPos;
    public int col;
    public int row;
    public ComponentDataFromEntity<NonUniformScale> nonuniforms;
    public DynamicBuffer<BoxesComponent> boxes;
    public int frames;

    public float dt;

    public void Execute(ref Player player, ref ParabolaComp parabola, in Translation translation)
    {
        parabola.t += dt;
        if (parabola.t < 1f) return; // discard player input if mid-bounce
        
        // Find closest box coords in hitPos direction
        int gridX = (int)math.round(hitPos.x);
        int gridY = (int)math.round(hitPos.z);
        //int gridBoxIndex = col * gridX + gridY;

        int playerGridX = (int)math.round(translation.Value.x);
        int playerGridY = (int)math.round(translation.Value.z);

        int targetX = gridX;
        int targetY = gridY;

        int targetBoxIndex = col * targetX + targetY;
        //BoxesComponent box = boxes[targetBoxIndex]; // FIXME: sanitize this?
        BoxesComponent box;
        try
        {
            box = boxes[targetBoxIndex];
        }
        catch (Exception e)
        {
            Debug.Log($"failed attempt to target box at index {targetBoxIndex}, " +
                      $"from coords x:{targetX} y:{targetY}");
            return;
        }

        if (math.abs(gridX - playerGridX) > 1 || math.abs(gridY - playerGridY) > 1)
        {
            targetX = playerGridX;
            targetY = playerGridY;

            // increments single step target position towards mouse position
            if (gridX != playerGridX)
            {
                targetX += gridX > playerGridX ? 1 : -1;
            }
            if (gridY != playerGridY)
            {
                targetY += gridY > playerGridY ? 1 : -1;
            }

            targetBoxIndex = col * targetX + targetY;
            box = boxes[targetBoxIndex];
            if (box.occupied)
            {
                targetX = playerGridX;
                targetY = playerGridY;
            }

            player.targetX = targetX;
            player.targetY = targetY;
        }
        
        // create/overwrite parabola...
        // t > 1 means bounce is complete (IDLE)
        // 0 < t < 1 means bounce is ongoing (BOUNCING)
        
        if (parabola.t >= 1.0f) // this check can be removed(?), see start of method-body
        {
            Debug.Log($"new bounce from {translation.Value.x}|{translation.Value.z} to {hitPos.x}|{hitPos.z}" +
                      $" - rounded to: {gridX}|{gridY}");
                
            Entity gridBoxEntity = box.entity;
            NonUniformScale gridBoxScale = nonuniforms[gridBoxEntity];
            float startY = gridBoxScale.Value.y; // + jump offset?

            //int targetBoxIndex = col * targetX + targetY;
            BoxesComponent targetBox = boxes[targetBoxIndex];
            Entity targetBoxEntity = targetBox.entity;
            NonUniformScale targetBoxScale = nonuniforms[targetBoxEntity];
            float endY = targetBoxScale.Value.y; // + jump offset?

            float height = math.max(startY, endY);
            height += parabola.BOUNCE_HEIGHT;
            
            // calculate new parabola! (overwrites data in parabola component)
            parabola.c = startY;

            float k = math.sqrt(math.abs(startY - height)) /
                      (math.sqrt(math.abs(startY - height)) +
                       math.sqrt(math.abs(endY - height)));

            parabola.a = (height - startY - k * (endY - startY)) / (k * k - k);
            parabola.b = endY - startY - parabola.a;
            parabola.t = 0f; // reset t to start new parabola movement
        } 
    }

    


        
        //if (gridBoxIndex >= boxes.Length) return; // Just testing, ensuring that we don't attempt a non-working index
        
        // Use box coords as index into buffer
        
        //Entity boxEntity = box.entity;

        // Use calculations from original code for height value of parabola and the parabola parameters
        //NonUniformScale boxScale = nonuniforms[boxEntity];

        //    boxTranslation.y; // use for height calculations of parabola, see original code in Player


        // Set parabola params...
        



        // if (frames % 60 == 0)
        // {
        //     // Debug.Log($"Hitpos: [{hitPos.x}, {hitPos.y}, {hitPos.z}]");
        //     // Debug.Log($"GridX: {gridX}");
        //     // Debug.Log($"GridY: {gridY}");
        //     Debug.Log($"BoxIndex: {boxIndex}");
        //     Debug.Log($"Box entity index: {boxEntity.Index}");
        //     Debug.Log($"Non-Uniform Scale: {boxScale.Value}");
        // }

}


