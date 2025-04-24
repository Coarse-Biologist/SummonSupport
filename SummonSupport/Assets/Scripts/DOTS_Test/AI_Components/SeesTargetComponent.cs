using Unity.Entities;
using Unity.Mathematics;

public struct SeesTargetComponent : IComponentData, IEnableableComponent
{
    public float3 targetLocation;
}
