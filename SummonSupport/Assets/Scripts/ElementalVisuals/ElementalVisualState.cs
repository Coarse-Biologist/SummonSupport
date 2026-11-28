using System;

/// <summary>
/// The whole input to a creature's elemental look: the same state always gives the same model, tint and colors.
/// </summary>
public struct ElementalVisualState : IEquatable<ElementalVisualState>
{
    public Element element;
    public int affinity;
    public int seed;

    public ElementalVisualState(Element element, int affinity, int seed)
    {
        this.element = element;
        this.affinity = affinity;
        this.seed = seed;
    }

    public bool Equals(ElementalVisualState other) => element == other.element && affinity == other.affinity && seed == other.seed;
    public override bool Equals(object obj) => obj is ElementalVisualState s && Equals(s);
    public override int GetHashCode() => HashCode.Combine((int)element, affinity, seed);
    public override string ToString() => $"{element} {affinity} (seed {seed})";
}

/// <summary>
/// Implement on a component of the creature root (same GameObject as <see cref="ElementalAppearance"/>)
/// to re-bind references when the model is swapped.
/// </summary>
public interface IElementalModelListener
{
    void OnElementalModelChanged(ElementalModel current, bool isBaseModel);
}
