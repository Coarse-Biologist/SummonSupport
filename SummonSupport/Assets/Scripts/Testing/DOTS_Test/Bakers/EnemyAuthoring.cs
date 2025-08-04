using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;


public class EnemyAuthoring : MonoBehaviour
{
    public float Speed = 0f;
    public Vector3 targetLocation;

    class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {

            //("Bake function is called");
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SpeedComponent { Speed = authoring.Speed });
            AddComponent(entity, new SeesTargetComponent { targetLocation = authoring.targetLocation });
            //SetComponentEnabled<SeesTargetComponent>(entity, false);

        }

    }

}

