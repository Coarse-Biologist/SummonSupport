using System;
using UnityEngine;

public class CreatureColorInfo : MonoBehaviour
{
    [field: SerializeField] public SkinnedMeshRenderer[] MeshRenderers { private set; get; }
}
