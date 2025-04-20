using System.Diagnostics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct MoveUpSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MoveUpComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (transform, move) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveUpComponent>>())
        {
            transform.ValueRW.Position.y += move.ValueRO.Speed * SystemAPI.Time.DeltaTime;

        }
    }
}
