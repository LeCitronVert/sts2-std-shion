using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Monsters;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

[Pool(typeof(ShionTheBruiserRelicPool))]
public class AbbyTheAbsol : CritContributorRelic, ICritHitRateContributor, IOnCriticalHitListener
{
    // Shion relic
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();

    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
    
    // Normal stuff I master
    public override RelicRarity Rarity =>
        RelicRarity.Rare;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CritDisplayTooltipVar(),
        new DynamicVar("CritAttackRate", 10)
    ];
    
    // Everything below this point is pet related stuff i copied from byrdpip and i have no idea wha ti'm doing

    public override bool AddsPet => true;
    public override bool SpawnsPets => true;

    public override async Task BeforeCombatStart() => await this.SummonPet();

    private async Task SummonPet()
    {
        await PlayerCmd.AddPet<AbbyMonster>(this.Owner);
    }

    public async Task OnCriticalHit(Creature attacker, Creature target, DamageResult damageResult, ValueProp props,
        CardModel? sourceCard, PlayerChoiceContext? choiceContext)
    {
        if (target.IsDead || Owner.PlayerCombatState == null || sourceCard == null || sourceCard.Owner != Owner) return;

        Flash();
        await Cmd.Wait(0.3f);
        await DamageCmd
            .Attack(damageResult.TotalDamage)
            .FromCard(sourceCard)
            .Unpowered()
            .Targeting(target)
            .WithAttackerAnim(
             "Attack", 
             Owner.Character.AttackAnimDelay, 
             Owner.PlayerCombatState.GetPet<AbbyMonster>()
            )
            .WithHitFx("vfx/vfx_attack_slash", Path.Join(MainFile.ResPath, "audio", "monsters", "abby_cry.mp3"))
            .Execute(choiceContext)
        ;
        await Cmd.Wait(0.3f);


    }

    public decimal GetCritHitRateBonus(CardModel? sourceCard)
    {
        if (sourceCard != null && sourceCard.Owner.Creature != Owner.Creature) return 0;
        
        var bonus = DynamicVars["CritAttackRate"].BaseValue / 100;
        return bonus;
    }
}