using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

public class MonsterMagicHelper(Creature monster)
{
    public Creature Monster = monster;

    // Attack
    public bool HasAttack;
    public int DamagePerHit;
    public int HitCount;

    // Block
    public bool HasBlock;
    public int BlockAmount;

    // Powers applied to self (the enemy who acted)
    public List<(PowerModel Power, int Amount)> PowersOnSelf = [];

    // Powers/debuffs applied to others (usually the player)
    public List<(PowerModel Power, int Amount)> PowersOnTarget = [];
}