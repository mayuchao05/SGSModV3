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

// 八卦阵：能力，费3（升级后 2），品质金，目标自身。
// 数据表：受到的所有伤害减少 50%（升级只降费，效果不变）。
// 实现：挂 BaGuaZhenPower（伤害 ×0.5）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class BaGuaZhenCard : SGSModV3BaseCard
{
    public BaGuaZhenCard() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        PowerCmd.Apply<BaGuaZhenPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 数据表：升级只降费（3 -> 2）
        base.EnergyCost.UpgradeBy(-1);
    }
}
