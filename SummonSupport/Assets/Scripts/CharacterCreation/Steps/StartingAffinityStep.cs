/// <summary>Sets the starting affinity for the chosen element (NetworkPlayer sends affinities to the host).</summary>
public class StartingAffinityStep : ICharacterCreationStep
{
    public void Apply(CharacterCreationContext context)
    {
        if (context.Data.Element == Element.None) return;
        context.Player.SetAffinity(context.Data.Element, context.Profile.StartingAffinity);
    }
}
