using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 青囊：技能，费0，品质白。
// 数据表：消耗。消耗1张手牌，回复3点生命（升级5点）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class QingNangCard : SGSModV3BaseCard
{
    public QingNangCard() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword> { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = Owner.PlayerCombatState?.Hand.Cards;
        if (hand == null || hand.Count == 0)
        {
            // 没有手牌可消耗：直接回血。
            await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue, true);
            return;
        }

        // 青囊自身已被打出，理论上不在手牌；防御性过滤掉正在打的这张牌。
        CardSelectorPrefs prefs = new(CardSelectorPrefs.ExhaustSelectionPrompt, 1, 1);
        IEnumerable<CardModel>? chosen = await CardSelectCmd.FromHand(
            choiceContext, Owner, prefs, c => c != cardPlay.Card, this);

        CardModel? toExhaust = chosen?.FirstOrDefault();
        if (toExhaust != null)
        {
            await CardCmd.Exhaust(choiceContext, toExhaust, false, false);
        }

        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue, true);
    }

    protected override void OnUpgrade() => DynamicVars.Heal.UpgradeValueBy(2m);
}
