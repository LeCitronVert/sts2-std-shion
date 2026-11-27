using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Strife;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Blue Chaos Emerald Burst (Emerald Power, 2-cost Power): gain 1 (2) energy. Choose a card in your Hand: until you play
/// it, it becomes an X-cost card with Replay X (<see cref="EmeraldAvatarBurst"/>).
/// </summary>
[Pool(typeof(TokenCardPool))]
public class EmeraldPowerAvatar() : EmeraldPowerCard(2, TargetType.Self)
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Blue;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.ReplayDynamic, new DynamicVar("Times", 1M))];

    protected override async Task OnEmeraldPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        var chosen = (await CardSelectCmd.FromHand(choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1),
            c => c is not EmeraldPowerCard, this)).FirstOrDefault();
        if (chosen == null) return;
        var creature = Owner.Creature;
        if (creature.GetPower<EmeraldAvatarBurst>() is null)
            await PowerCmd.Apply<EmeraldAvatarBurst>(choiceContext, creature, 1M, creature, this);
        creature.GetPower<EmeraldAvatarBurst>()?.Choose(chosen);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}
