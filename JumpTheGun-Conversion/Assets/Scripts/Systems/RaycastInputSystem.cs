using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

[UpdateAfter(typeof(SpawnTerrainSystem))]
public partial class RaycastInputSystem : SystemBase
{
    public BeginSimulationEntityCommandBufferSystem ecbSystem;
    public BuildPhysicsWorld _physicsWorld;
    public CollisionWorld _collisionWorld;

    // public GameData _gameData;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        _physicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        RequireSingletonForUpdate<Player>();
    }

    // protected override void OnStartRunning()
    // {
    //     if (_gameData.Equals(null))
    //     {
    //         _gameData = GetSingleton<GameData>(); // OBS: this approach is currently not working or active
    //     }
    // }

    protected override void OnUpdate()
    {
        float dt = Time.DeltaTime;

        // Debugging things...
        // if (Input.GetKey(KeyCode.A))
        //     Debug.Break();

        RaycastInput raycastInput = new RaycastInput();
        RaycastHit raycastHit;

        if (Camera.main != null)
        {
            _collisionWorld = _physicsWorld.PhysicsWorld.CollisionWorld;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var rayOrigin = ray.origin;
            var rayEnd = ray.GetPoint(30f);

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
            var nonuniforms = GetComponentDataFromEntity<NonUniformScale>(true);

            // Schedule player direction job
            var playerDirJob = new PlayerDirectionJob
            {
                hitPos = hitPos,
                col = gameData.width,
                row = (int)gameData.height,
                boxes = GetBuffer<BoxesComponent>(gameData.manager, true),
                nonuniforms = nonuniforms,
                // frames = UnityEngine.Time.frameCount,
                dt = dt
            };

            var handle = playerDirJob.Schedule();
            ecbSystem.AddJobHandleForProducer(handle);
        }
    }
}

[BurstCompile]
public partial struct PlayerDirectionJob : IJobEntity
{
    [ReadOnly] public float3 hitPos;
    [ReadOnly] public int col;
    [ReadOnly] public int row;
    [ReadOnly] public ComponentDataFromEntity<NonUniformScale> nonuniforms;

    [ReadOnly] public DynamicBuffer<BoxesComponent> boxes;
    // public int frames;

    [ReadOnly] public float dt;

    public void Execute(ref Player player, ref ParabolaComp parabola, in Translation translation)
    {
        parabola.t += dt;
        if (parabola.t < 1f) return; // discard player input if mid-bounce

        // Find closest box coords in hitPos direction
        int mouseGridX = (int)math.round(hitPos.x);
        int mouseGridY = (int)math.round(hitPos.z);

        int playerGridX = (int)math.round(translation.Value.x);
        int playerGridY = (int)math.round(translation.Value.z);
        player.currentX = playerGridX;
        player.currentY = playerGridY;

        int targetX = mouseGridX;
        int targetY = mouseGridY;

        if (math.abs(mouseGridX - playerGridX) > 1 || math.abs(mouseGridY - playerGridY) > 1)
        {
            targetX = playerGridX;
            targetY = playerGridY;

            // increments single step target position towards mouse position
            if (mouseGridX != playerGridX)
            {
                targetX += mouseGridX > playerGridX ? 1 : -1;
            }

            if (mouseGridY != playerGridY)
            {
                targetY += mouseGridY > playerGridY ? 1 : -1;
            }
        }

        // Avoid going outside the grid, col and row -1 because 0-indexed array
        if (targetX < 0 || targetX > col - 1) targetX = playerGridX;
        if (targetY < 0 || targetY > row - 1) targetY = playerGridY;

        int currentBoxIndex = col * playerGridX + playerGridY;
        int targetBoxIndex = col * targetX + targetY;
        BoxesComponent targetBox = boxes[targetBoxIndex];

        if (targetBox.occupied)
        {
            targetX = playerGridX;
            targetY = playerGridY;
        }

        player.isTargetOccupied = targetBox.occupied; // TODO: clean up redundant references
        player.targetX = targetX;
        player.targetY = targetY;


        // create/overwrite parabola...
        // t > 1 means bounce is complete (IDLE)
        // 0 < t < 1 means bounce is ongoing (BOUNCING)

        if (parabola.t >= 1.0f) // this check can be removed(?), see start of method-body
        {
            // access start height:
            NonUniformScale currentBoxScale = nonuniforms[boxes[currentBoxIndex].entity];
            float startY = currentBoxScale.Value.y;
            float endY;

            if (targetBox.occupied)
            {
                endY = startY; // fixed bounce height with bouncing in place when target occupied
            }
            else
            {
                // access end height:
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