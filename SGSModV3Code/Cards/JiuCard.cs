using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 酒：技能，费1（升级后 0），品质蓝。
// 数据表：你下一张攻击牌伤害翻倍（升级只把费用 1 -> 0，效果不变）。
// 实现：挂 1 层 JiuDoubleDamagePower（仿超巨化药水机制，倍率 2，用完即移除）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class JiuCard : SGSModV3BaseCard
{
    public JiuCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        PowerCmd.Apply<JiuDoubleDamagePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 数据表：升级只降费（1 -> 0），效果不变
        base.EnergyCost.UpgradeBy(-1);
    }
}
