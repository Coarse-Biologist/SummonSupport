/// <summary>
/// One self-contained part of making a new character (name, affinity, knowledge, minions...).
/// To extend character creation, implement this and add it to <see cref="CharacterCreationPipeline.CreateDefaultSteps"/>.
/// Steps run on the machine that owns the player, once the player and the level have started.
/// </summary>
public interface ICharacterCreationStep
{
    void Apply(CharacterCreationContext context);
}

/// <summary>Everything a step may need. Add properties here rather than reaching for singletons in steps.</summary>
public sealed class CharacterCreationContext
{
    public PlayerStats Player { get; }
    public CharacterCreationData Data { get; }
    public CharacterCreationProfile_SO Profile { get; }

    public CharacterCreationContext(PlayerStats player, CharacterCreationData data, CharacterCreationProfile_SO profile)
    {
        Player = player;
        Data = data;
        Profile = profile;
    }
}
