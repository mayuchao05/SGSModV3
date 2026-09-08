using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 五谷丰登：消耗，全体友方摸 2 张牌。升级后费用降低（效果不变）。
// 简单结算卡：对玩家抽牌（单人 MOD 友方即自身）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class WuGuFengDengCard : SGSModV3BaseCard
{
    public WuGuFengDengCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2)
    ];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 效果不变。表中升级后费用降为 0，但当前基类未暴露费用升级 API；
        // 暂以保持费用不变实现，费用降级将在后续批次补全。
    }
}
