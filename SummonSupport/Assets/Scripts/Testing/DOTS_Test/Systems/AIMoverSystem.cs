using System.Diagnostics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Jobs;

//[BurstCompile]
//public partial struct AIMoverSystem : ISystem
//{
//    public void OnCreate(ref SystemState state)
//    {
//        state.RequireForUpdate<MoveUpComponent>();
//    
//    [BurstCompile]
//    public void OnUpdate(ref SystemState state)
//    {
//        AIMoverJob moverJob = new AIMoverJob { deltaTime = SystemAPI.Time.DeltaTime };
//        moverJob.ScheduleParallel();
//    
//    foreach (var (transform, sees, speed, physics) in SystemAPI.Query<RefRW<LocalTransform>,
//     RefRO<SeesTargetComponent>,
//     RefRO<SpeedComponent>,
//     RefRW<PhysicsVelocity>>())
//    {
//        float3 moveDirection = sees.ValueRO.targetLocation - transform.ValueRW.Position;
//        moveDirection = math.normalize(moveDirection);
//        //transform.ValueRW.Position = moveDirection * speed.ValueRO.Speed;
//        physics.ValueRW.Linear = moveDirection * speed.ValueRO.Speed;
//    
//    }
//    
//    }
//    }  
//}
//
//[BurstCompile]
//public partial struct AIMoverJob : IJobEntity
//
//{
//    public float deltaTime;
//    public void Execute(in LocalTransform transform, ref SeesTargetComponent sees, in SpeedComponent speed, ref PhysicsVelocity physics)
//    {
//        float distanceSqToTarget = 2f;
//        float3 moveDirection = sees.targetLocation - transform.Position;
//
//        if (math.lengthsq(moveDirection) <= distanceSqToTarget)
//        {
//            physics.Linear = float3.zero;
//            return;
//        }
//        else
//        {
//            moveDirection = math.normalize(moveDirection);
//            physics.Linear = moveDirection * speed.Speed;
//        }
//    }
//}
