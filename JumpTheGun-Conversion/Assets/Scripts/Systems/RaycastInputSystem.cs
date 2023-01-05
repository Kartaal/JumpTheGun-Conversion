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

    public GameData _gameData;
    
    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        _physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        RequireSingletonForUpdate<Player>();
    }

    protected override void OnStartRunning()
    {
        if (_gameData.Equals(null))
        {
            _gameData = GetSingleton<GameData>(); // OBS: this approach is currently not working or active
        } 
    }

    protected override void OnUpdate()
    {
        float dt = Time.DeltaTime;
        
        // Debugging things...
        if(Input.GetKey(KeyCode.A))
            Debug.Break();
        
        RaycastInput raycastInput = new RaycastInput();
        RaycastHit raycastHit;

        if (Camera.main != null)
        {
            _collisionWorld = _physicsWorld.PhysicsWorld.CollisionWorld;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var rayOrigin = ray.origin;
            var rayEnd = ray.GetPoint(30f);
            //var rayEnd = ray.GetPoint(_gameData.raycastDistance);

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
                // frames = UnityEngine.Time.frameCount,
                dt = dt
            };

            var handle = playerDirJob.Schedule();
            ecbSystem.AddJobHandleForProducer(handle);
        }
    }
}

//[BurstCompile]
public partial struct PlayerDirectionJob : IJobEntity
{
    public float3 hitPos;
    public int col;
    public int row;
    public ComponentDataFromEntity<NonUniformScale> nonuniforms;
    public DynamicBuffer<BoxesComponent> boxes;
    // public int frames;

    public float dt;

    public void Execute(ref Player player, ref ParabolaComp parabola, in Translation translation)
    {
        parabola.t += dt;
        if (parabola.t < 1f) return; // discard player input if mid-bounce
        
        // Find closest box coords in hitPos direction
        int gridX = (int)math.round(hitPos.x);
        int gridY = (int)math.round(hitPos.z);

        int playerGridX = (int)math.round(translation.Value.x);
        int playerGridY = (int)math.round(translation.Value.z);
        player.currentX = playerGridX;
        player.currentY = playerGridY;

        int targetX = gridX;
        int targetY = gridY;

        // int targetBoxIndex = col * targetX + targetY;
        // //BoxesComponent box = boxes[targetBoxIndex]; // FIXME: sanitize this?
        // BoxesComponent box;
        // try
        // {
        //     box = boxes[targetBoxIndex];
        // }
        // catch (Exception e)
        // {
        //     Debug.Log($"failed attempt to target box at index {targetBoxIndex}, " +
        //               $"from coords x:{targetX} y:{targetY}");
        //     return;
        // }

        bool targetOccupied = false;

        // Debug.Log($"Rounded hit: ({gridX}, {gridY})");
        // Debug.Log($"Player grid: ({playerGridX}, {playerGridY})");
        // Debug.Log($"X offset: {math.abs(gridX - playerGridX)}");
        // Debug.Log($"Y offset: {math.abs(gridY - playerGridY)}");
        // Debug.Log("");
        
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
        }

        // Avoid going outside the grid, col and row -1 because 0-indexed array
        if (targetX < 0 || targetX > col-1) targetX = playerGridX;
        if (targetY < 0 || targetY > row-1) targetY = playerGridY;

        int currentBoxIndex = col * playerGridX + playerGridY;
        int targetBoxIndex = col * targetX + targetY;
        BoxesComponent targetBox = boxes[targetBoxIndex];
        
        if (targetBox.occupied)
        {
            targetX = playerGridX;
            targetY = playerGridY;
            targetOccupied = true; // only used for debugging
        }
        
        player.isTargetOccupied = targetBox.occupied; // TODO: clean up redundant references
        player.targetX = targetX;
        player.targetY = targetY;
        
        
        // create/overwrite parabola...
        // t > 1 means bounce is complete (IDLE)
        // 0 < t < 1 means bounce is ongoing (BOUNCING)
        
        if (parabola.t >= 1.0f) // this check can be removed(?), see start of method-body
        {
            Debug.Log($"new bounce from {player.currentX}|{player.currentY} to {hitPos.x}|{hitPos.z}" +
                      $" - rounded to: {gridX}|{gridY} - target occupied = {targetOccupied}");
                
            //Entity gridBoxEntity = targetBox.entity;
            //NonUniformScale gridBoxScale = nonuniforms[gridBoxEntity];
            //float startY = gridBoxScale.Value.y; // + jump offset?

            // access start height:
            //BoxesComponent currentBox = boxes[currentBoxIndex];
            //Entity currentBoxEntity = currentBox.entity;
            NonUniformScale currentBoxScale = nonuniforms[boxes[currentBoxIndex].entity];
            float startY = currentBoxScale.Value.y;
            float endY;
            
            if (targetBox.occupied)
            {
                endY = startY; // fixed bounce height to bounce in place when target occupied
            } else {
                // access end height:
                //BoxesComponent targetBoxComp = boxes[targetBoxIndex];
                //Entity targetBoxEntity = targetBoxComp.entity;
                NonUniformScale targetBoxScale = nonuniforms[boxes[targetBoxIndex].entity];
                endY = targetBoxScale.Value.y;    
            }

            float height = math.max(startY, endY);
            height += parabola.BOUNCE_HEIGHT;
            
            // calculate new parabola! (overwrites data in player's parabola component)
            parabola.c = startY;

            float k = math.sqrt(math.abs(startY - height)) /
                      (math.sqrt(math.abs(startY - height)) +
                       math.sqrt(math.abs(endY - height)));

            parabola.a = (height - startY - k * (endY - startY)) / (k * k - k);
            parabola.b = endY - startY - parabola.a;
            parabola.t = 0f; // reset t to start new parabola movement
        } 
    }
}


