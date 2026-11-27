/// <summary>Sets the starting alchemic knowledge for the chosen element. Each player has their own alchemy inventory.</summary>
public class StartingKnowledgeStep : ICharacterCreationStep
{
    public void Apply(CharacterCreationContext context)
    {
        if (context.Data.Element == Element.None) return;
        AlchemyInventory.SetElementalKnowledge(context.Data.Element, context.Profile.StartingKnowledge);
    }
}
