using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 白银狮子：技能，费1，品质蓝，消耗。
// 数据表：获得 10 点护甲，下回合开始时回复 3 点血量（升级后 13 点护甲，回复 5 点）。
// 实现：护甲立即获得（BlockVar）；延迟回血挂 BaiYinShiZiHealPower（下回合开始触发一次后自移除）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class BaiYinShiZiCard : SGSModV3BaseCard
{
    private const string NextTurnHealKey = "NextTurnHeal";

    public BaiYinShiZiCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 「消耗」是卡牌特性，走 Keyword 驱动（卡面标签 + 打出后移出本场战斗），不写进描述文本。
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword> { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(10m, ValueProp.Move),
        new DynamicVar(NextTurnHealKey, 3m)
    ];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 立即获得护甲
        CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        // 挂上「下回合开始时回血」的能力
        int heal = base.DynamicVars[NextTurnHealKey].IntValue;
        PowerCmd.Apply<BaiYinShiZiHealPower>(choiceContext, Owner.Creature, heal, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);          // 10 -> 13
        base.DynamicVars[NextTurnHealKey].UpgradeValueBy(2m); // 3 -> 5
    }
}
