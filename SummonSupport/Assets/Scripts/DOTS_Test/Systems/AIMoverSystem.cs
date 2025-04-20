using System.Diagnostics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

[BurstCompile]
public partial struct AIMoverSystem : ISystem
{
    //public void OnCreate(ref SystemState state)
    //{
    //    state.RequireForUpdate<MoveUpComponent>();
    //}

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (transform, sees, speed, physics) in SystemAPI.Query<RefRW<LocalTransform>,
         RefRO<SeesTargetComponent>,
         RefRO<SpeedComponent>,
         RefRW<PhysicsVelocity>>())
        {
            float3 moveDirection = sees.ValueRO.targetLocation - transform.ValueRW.Position;
            moveDirection = math.normalize(moveDirection);
            //transform.ValueRW.Position = moveDirection * speed.ValueRO.Speed;
            physics.ValueRW.Linear = moveDirection * speed.ValueRO.Speed;

        }
    }
}
