using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionMonsterSpell() : ShionTheBruiserCard(3,
    CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
{
    private bool _hasAttack;
    private int _damagePerHit;
    private int _hitCount = 1;

    private bool _hasBlock;
    private int _blockAmount;

    private List<(Type PowerType, int Amount)> _powersOnSelf = [];
    private List<(Type PowerType, int Amount)> _powersOnTarget = [];
    
    public override bool CanBeGeneratedInCombat => false;
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Ethereal,
        CardKeyword.Exhaust,
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("HasAttack", _hasAttack ? 1 : 0),
        new DamageVar(_damagePerHit, ValueProp.Move),
        new RepeatVar(_hitCount),
        new DynamicVar("HasBlock", _hasBlock ? 1 : 0),
        new BlockVar(_blockAmount, ValueProp.Move),
        new DynamicVar("PowersOnSelfAmount", _powersOnSelf.Count),
        new DynamicVar("PowersOnTargetAmount", _powersOnTarget.Count)
    ];

    private void UpdateDynamicVars()
    {
        DynamicVars["HasAttack"].BaseValue = _hasAttack ? 1 : 0;
        DynamicVars.Damage.BaseValue = _damagePerHit;
        DynamicVars.Repeat.BaseValue = _hitCount;
        DynamicVars["HasBlock"].BaseValue = _hasBlock ? 1 : 0;
        DynamicVars.Block.BaseValue = _blockAmount;
        DynamicVars["PowersOnSelfAmount"].BaseValue = _powersOnSelf.Count;
        DynamicVars["PowersOnTargetAmount"].BaseValue = _powersOnTarget.Count;
    }


    public void Configure(MonsterMagicHelper memory)
    {
        _hasAttack = memory.HasAttack;
        _damagePerHit = memory.DamagePerHit;
        _hitCount = memory.HitCount;

        _hasBlock = memory.HasBlock;
        _blockAmount = memory.BlockAmount;

        _powersOnSelf = memory.PowersOnSelf;
        _powersOnTarget = memory.PowersOnTarget;
        
        UpdateDynamicVars();
    }


    public override TargetType TargetType =>
        (_hasAttack || _powersOnTarget.Count > 0) ? TargetType.AnyEnemy : TargetType.Self;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var card = play.Card;
        
        if (_hasAttack)
        {
            ArgumentNullException.ThrowIfNull(play.Target);
            await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
                .WithHitCount(card.DynamicVars.Repeat.IntValue)
                .FromCard(card)
                .Targeting(play.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext)
            ;
        }

        if (_hasBlock)
        {
            await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.Block, play);
        }

        foreach (var (powerType, amount) in _powersOnTarget)
            await MonsterPowersHelper.Apply(choiceContext, powerType, play.Target!, amount, play.Card.Owner.Creature, card);

        foreach (var (powerType, amount) in _powersOnSelf)
            await MonsterPowersHelper.Apply(choiceContext, powerType, play.Card.Owner.Creature, amount, play.Card.Owner.Creature, card);
    }

    protected override void OnUpgrade() { }
}