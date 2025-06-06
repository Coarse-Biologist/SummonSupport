using UnityEngine;

public interface I_Combatant
{
    void TakeDamage(Element element, int value);
    void RecieveHeal(int value);
    void RecieveBuff(string buff);
    void RecieveDebuff(string debuff);
    void RecieveStatusEffect(string statusEffect);

}
