/// <summary>Gives the player the name chosen on the start screen (NetworkPlayer shares it with everyone online).</summary>
public class PlayerNameStep : ICharacterCreationStep
{
    public void Apply(CharacterCreationContext context)
    {
        if (!string.IsNullOrEmpty(context.Data.PlayerName)) context.Player.SetName(context.Data.PlayerName);
    }
}
