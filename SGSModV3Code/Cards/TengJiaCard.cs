using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 藤甲：能力，费1，品质蓝。
// 数据表：获得 5 层覆甲，失去 1 点敏捷（升级后 7 层覆甲，失去 1 点敏捷）。
// 实现：覆甲与敏捷损失都放在 DynamicVar 里，出牌时读取并手动施加。
// 说明：覆甲用原生 PlatingPower；敏捷用 DexterityPower（负值即减敏捷）。
//       描述令牌 {Plating:diff()} / {DexterityLoss:diff()} 依赖 CanonicalVars，之前为空导致数字不显示。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class TengJiaCard : SGSModV3BaseCard
{
    private const string PlatingKey = "Plating";
    private const string DexterityLossKey = "DexterityLoss";

    public TengJiaCard() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new DynamicVar(PlatingKey, 5m),
        new DynamicVar(DexterityLossKey, 1m)
    };

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int plating = base.DynamicVars[PlatingKey].IntValue;
        int dexLoss = base.DynamicVars[DexterityLossKey].IntValue;

        // 获得覆甲
        PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature, plating, Owner.Creature, this, false);
        // 失去敏捷（负敏捷）
        PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, -dexLoss, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars[PlatingKey].UpgradeValueBy(2m);
    }
}
