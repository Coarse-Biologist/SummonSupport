using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;


public class EnemyAuthoring : MonoBehaviour
{
    public float Speed = 1f;

    class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {

            Debug.Log("Bake function is called");
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new MoveUpComponent { Speed = authoring.Speed });
        }

    }

}

