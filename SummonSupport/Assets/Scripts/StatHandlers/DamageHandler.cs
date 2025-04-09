
public static class DamageHandler
{
    public static void ApplyDamage(LivingBeing target, Ability ability)
    {
        target.ChangeAttribute(AttributeType.CurrentHitpoints, ability.Value);
    }
}