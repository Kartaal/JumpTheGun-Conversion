using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class ApplyClass
{
    [BurstCompile]
    public partial struct ApplyNewScale : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<TriggerApplyScaleTag> allApplyScaleTag;
        [ReadOnly] public ComponentDataFromEntity<TriggerAffectedTag> allAffectedTag;
        public EntityCommandBuffer ecb;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;

            if (allApplyScaleTag.HasComponent(entityA) && allAffectedTag.HasComponent(entityB))
            {
                ecb.AddComponent(entityB, new NonUniformScale
                {
                    Value = new float3(3, 3, 3)
                });
            }
            else if (allApplyScaleTag.HasComponent(entityB) && allAffectedTag.HasComponent(entityA))
            {
                ecb.AddComponent(entityA, new NonUniformScale
                {
                    Value = new float3(3, 3, 3)
                });
            }
        }
    }

    [BurstCompile]
    public partial struct ApplyRotTag : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<TriggerRotTag> allTriggerRotTags;
        [ReadOnly] public ComponentDataFromEntity<TriggerAffectedTag> allAffectedTag;
        public EntityCommandBuffer ecb;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;

            if (allTriggerRotTags.HasComponent(entityA) && allAffectedTag.HasComponent(entityB))
            {
                // ecb.AddComponent<RotateTag>(entityB);
                // ecb.AddComponent(entityB, new WaveDataComponent
                // {
                //     rotationSpeed = 100
                // });
                Debug.Log("print1");
            }
            else if (allTriggerRotTags.HasComponent(entityB) && allAffectedTag.HasComponent(entityA))
            {
                // ecb.AddComponent<RotateTag>(entityA);
                // ecb.AddComponent(entityA, new WaveDataComponent
                // {
                //     rotationSpeed = 100
                // });
                Debug.Log("print2");
            }
        }
    }
}