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

    // Power
    public List<(Type PowerType, int Amount)> PowersOnSelf = [];
    public List<(Type PowerType, int Amount)> PowersOnTarget = [];
}