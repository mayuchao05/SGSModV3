using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 五谷丰登：联机专属，消耗，所有友方抽 2 张牌。升级后抽 3 张。
// 参考官方联机专属卡 GlimpseBeyond（彼岸一瞥）：通过 MultiplayerConstraint 限制仅在多人模式出现。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class WuGuFengDengCard : SGSModV3BaseCard
{
    public WuGuFengDengCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies, true)
    {
    }

    // 联机专属：单人模式下不会出现在卡池、奖励、商店、转化等任何选牌界面。
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    // 「消耗」是卡牌特性，走 Keyword 驱动，不写进描述文本。
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword> { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 所有友方（含自己）抽牌。
        ICombatState? combatState = Owner.Creature.CombatState;
        if (combatState == null)
            return;

        foreach (Player player in combatState.Players)
        {
            if (player == null)
                continue;

            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, player, false);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}
