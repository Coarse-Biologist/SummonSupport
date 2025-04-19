using Unity.Entities;
using UnityEngine;
using Unity.Transforms;


public class CubeAuthoring : MonoBehaviour
{
    public float Speed = 1f;

    class Baker : Baker<CubeAuthoring>
    {
        public override void Bake(CubeAuthoring authoring)
        {
            //LocalTransform localTransform = null;
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MoveUpComponent { Speed = authoring.Speed });
        }

    }
}
