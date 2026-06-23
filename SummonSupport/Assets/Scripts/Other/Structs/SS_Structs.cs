
using System;
namespace SS_Structs
{
    [Serializable]
    public struct ElementKnowledge
    {
        public Element elementType;
        public int value;
    }

    [Serializable]
    public struct ElementAffinity
    {
        public Element elementType;
        public int value;

    }

    [Serializable]
    public struct CraftingPotentialDict
    {
        public CraftingPotential craftingPotential;
        public int value;
    }

    [Serializable]
    public struct SlottedAbilities
    {
        public int slot;
        public Ability ability;
    }



}
