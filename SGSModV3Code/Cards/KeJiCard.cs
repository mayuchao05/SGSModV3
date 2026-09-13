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

// 克己：能力，费1（升级后 0），品质蓝，目标自身。
// 数据表：回合结束时若你本回合没有打出攻击牌，则保留手牌（升级只降费，效果不变）。
// 实现：挂 KeJiRetainPower（判定 + 保留都在这个 Power 里）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class KeJiCard : SGSModV3BaseCard
{
    public KeJiCard() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        PowerCmd.Apply<KeJiRetainPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 数据表：升级只降费（1 -> 0）
        base.EnergyCost.UpgradeBy(-1);
    }
}
