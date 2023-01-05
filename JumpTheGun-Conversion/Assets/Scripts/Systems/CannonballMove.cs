using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateAfter(typeof(CannonballSpawningSystem))]
public partial class CannonballMove : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem ecbSystem;

    public GameData _gameData;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        RequireSingletonForUpdate<CannonballData>();
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
        float deltaTime = Time.DeltaTime;
        var gameData = GetSingleton<GameData>();
        var player = GetSingleton<Player>();

        // var cannonballSpawnJob = new CannonballMoveJob
        // {
        //     dt = deltaTime
        // };
        // var handle = cannonballSpawnJob.Schedule();

        // Schedule player direction job
        var playerDirJob = new CannonballParabolaMoveJob
        {
            col = gameData.width,
            row = (int)gameData.height,
            playerTargetX = player.targetX,
            playerTargetY = player.targetY,
            boxes = GetBuffer<BoxesComponent>(gameData.manager),
            // frames = UnityEngine.Time.frameCount,
            dt = deltaTime
        };

        var handle = playerDirJob.Schedule();
        ecbSystem.AddJobHandleForProducer(handle);
    }
}

//[BurstCompile]
public partial struct CannonballParabolaMoveJob : IJobEntity
{
    public float3 hitPos;
    public int col;

    public int row;

    // public ComponentDataFromEntity<NonUniformScale> nonuniforms;
    public DynamicBuffer<BoxesComponent> boxes;
    // public int frames;

    public float dt;
    public int playerTargetX;
    public int playerTargetY;

    public void Execute(ref CannonballData cannonball, ref ParabolaComp parabola, in Translation translation)
        // public void Execute(ref Player player, ref ParabolaComp parabola, in Translation translation)
    {
        parabola.t += dt;
        if (parabola.t < 1f) return; // discard player input if mid-bounce

        // Find closest box coords in hitPos direction
        // int mouseGridX = (int)math.round(hitPos.x);
        // int mouseGridY = (int)math.round(hitPos.z);
        //
        // int playerGridX = (int)math.round(translation.Value.x);
        // int playerGridY = (int)math.round(translation.Value.z);
        // player.currentX = playerGridX;
        // player.currentY = playerGridY;
        //
        // int targetX = mouseGridX;
        // int targetY = mouseGridY;
        //
        // bool targetOccupied = false;
        //
        // if (math.abs(mouseGridX - playerGridX) > 1 || math.abs(mouseGridY - playerGridY) > 1)
        // {
        //     targetX = playerGridX;
        //     targetY = playerGridY;
        //
        //     // increments single step target position towards mouse position
        //     if (mouseGridX != playerGridX)
        //     {
        //         targetX += mouseGridX > playerGridX ? 1 : -1;
        //     }
        //
        //     if (mouseGridY != playerGridY)
        //     {
        //         targetY += mouseGridY > playerGridY ? 1 : -1;
        //     }
        // }
        //
        // // Avoid going outside the grid, col and row -1 because 0-indexed array
        // if (targetX < 0 || targetX > col - 1) targetX = playerGridX;
        // if (targetY < 0 || targetY > row - 1) targetY = playerGridY;
        //
        // int currentBoxIndex = col * playerGridX + playerGridY;
        // int targetBoxIndex = col * targetX + targetY;
        int targetBoxIndex = col * playerTargetX + playerTargetY;
        BoxesComponent targetBox = boxes[targetBoxIndex];
        //
        // if (targetBox.occupied)
        // {
        //     targetX = playerGridX;
        //     targetY = playerGridY;
        //     targetOccupied = true; // only used for debugging
        // }

        // player.isTargetOccupied = targetBox.occupied; // TODO: clean up redundant references
        // player.targetX = targetX;
        // player.targetY = targetY;

        cannonball.currentX = playerTargetX;
        cannonball.currentY = playerTargetY;

        if (parabola.t >= 1.0f) // this check can be removed(?), see start of method-body
        {
            // access start height:
            // NonUniformScale currentBoxScale = nonuniforms[boxes[currentBoxIndex].entity];
            // float startY = currentBoxScale.Value.y;
            float startY = translation.Value.y;
            float endY = playerTargetY;

            if (targetBox.occupied)
            {
                endY = startY; // fixed bounce height with bouncing in place when target occupied
            }
            else
            {
                // access end height:
                // NonUniformScale targetBoxScale = nonuniforms[boxes[targetBoxIndex].entity];
                // endY = targetBoxScale.Value.y;
                endY = playerTargetY;
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

public partial struct CannonballMoveJob : IJobEntity
{
    public float dt;

    private void Execute(ref Translation trans, ref Rotation rot, ref CannonballData spawner)
    {
        trans.Value += spawner.speed * math.forward(rot.Value) * dt;
    }
}