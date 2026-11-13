using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper
{
    /// <summary>
    /// Bridges the game API differences between the main branch and the beta (v0.113+, built with STS2_BETA).
    /// Call sites are written against the beta API, either through these wrappers or directly where
    /// <see cref="Sts2MainBranchShims"/> supplies the beta overload on main, so the same code compiles on both.
    /// When the beta ships to main: inline these wrappers to the game methods, delete this file, and drop the
    /// <c>#else</c> branches of <c>#if STS2_BETA</c>.
    /// </summary>
    public static class Sts2Compat
    {
        /// <summary>Beta's damage from a card with no dealer (main: <c>Damage(ctx, target, amount, props, cardSource)</c>).</summary>
        public static Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, Creature target, decimal amount,
            ValueProp props, CardModel cardSource, CardPlay? cardPlay)
        {
#if STS2_BETA
            return CreatureCmd.Damage(choiceContext, target, amount, props, cardSource, cardPlay);
#else
            return CreatureCmd.Damage(choiceContext, target, amount, props, cardSource);
#endif
        }

        public static Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, Creature target, decimal amount,
            ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
        {
#if STS2_BETA
            return CreatureCmd.Damage(choiceContext, target, amount, props, dealer, cardSource, cardPlay);
#else
            return CreatureCmd.Damage(choiceContext, target, amount, props, dealer, cardSource);
#endif
        }

        public static Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, IEnumerable<Creature> targets, decimal amount,
            ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
        {
#if STS2_BETA
            return CreatureCmd.Damage(choiceContext, targets, amount, props, dealer, cardSource, cardPlay);
#else
            return CreatureCmd.Damage(choiceContext, targets, amount, props, dealer, cardSource);
#endif
        }

        /// <summary>Main: <c>LoseBlock(target, amount)</c>; beta adds the choice context and who removed the Block.</summary>
        public static Task LoseBlock(PlayerChoiceContext choiceContext, Creature target, decimal amount, Creature? remover)
        {
#if STS2_BETA
            return CreatureCmd.LoseBlock(choiceContext, target, amount, remover);
#else
            return CreatureCmd.LoseBlock(target, amount);
#endif
        }

        /// <summary>Main takes <c>isFromCard</c>; beta takes the card itself (null when not from a card).</summary>
        public static Task LoseMaxHp(PlayerChoiceContext choiceContext, Creature creature, decimal amount, CardModel? cardSource)
        {
#if STS2_BETA
            return CreatureCmd.LoseMaxHp(choiceContext, creature, amount, cardSource);
#else
            return CreatureCmd.LoseMaxHp(choiceContext, creature, amount, cardSource != null);
#endif
        }

        public static decimal ModifyDamage(IRunState runState, ICombatState? combatState, Creature? target, Creature? dealer,
            decimal damage, ValueProp props, CardModel? cardSource, CardPlay? cardPlay, ModifyDamageHookType hookType,
            CardPreviewMode previewMode, out IEnumerable<AbstractModel> modifiers)
        {
#if STS2_BETA
            return Hook.ModifyDamage(runState, combatState, target, dealer, damage, props, cardSource, cardPlay, hookType, previewMode, out modifiers);
#else
            return Hook.ModifyDamage(runState, combatState, target, dealer, damage, props, cardSource, hookType, previewMode, out modifiers);
#endif
        }

        /// <summary>
        /// Card creation options limited to exactly these (canonical) cards. Beta removed the custom card list,
        /// so there it's their pools filtered down to them.
        /// </summary>
        public static CardCreationOptions FromCards(IEnumerable<CardModel> cards, CardCreationSource source, CardRarityOddsType rarityOdds)
        {
#if STS2_BETA
            var list = cards.ToList();
            var ids = list.Select(c => c.Id).ToHashSet();
            return new CardCreationOptions(list.Select(c => c.Pool).Distinct(), source, rarityOdds, c => ids.Contains(c.Id));
#else
            return new CardCreationOptions(cards, source, rarityOdds);
#endif
        }

        /// <summary>A random relic off the front of the player's relic list, rarity rolled with their Rewards RNG (main's default).</summary>
        public static RelicModel PullNextRelicForReward(Player player)
        {
#if STS2_BETA
            return RelicFactory.PullNextRelicForRewardFromFront(player);
#else
            return RelicFactory.PullNextRelicFromFront(player);
#endif
        }
    }
}

#if !STS2_BETA
namespace ShionTheBruiser.ShionTheBruiserCode
{
    /// <summary>
    /// Beta overloads that don't exist on main, so call sites can use the beta form on both branches.
    /// In the mod's root namespace so every file sees these extensions without a using.
    /// </summary>
    public static class Sts2MainBranchShims
    {
        /// <summary>Beta passes the card play along to the attack; main has no such parameter.</summary>
        public static AttackCommand FromCard(this AttackCommand attack, CardModel card, CardPlay? cardPlay) => attack.FromCard(card);

        /// <summary>Beta names the chooser; main infers it.</summary>
        public static Task SignalPlayerChoiceBegun(this PlayerChoiceContext context, Player chooser, PlayerChoiceOptions options) =>
            context.SignalPlayerChoiceBegun(options);
    }
}
#endif
