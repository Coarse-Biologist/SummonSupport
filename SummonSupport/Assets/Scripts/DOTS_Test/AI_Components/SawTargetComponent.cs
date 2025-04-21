using Unity.Entities;
using Unity.Mathematics;

public struct SawTargetComponent : IComponentData, IEnableableComponent
{
    public float3 targetLocation;
}
