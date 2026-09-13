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

// 借刀杀人：技能，费1（升级后 0），品质白。
// 数据表：获得 1 点力量，敌人失去 1 点力量；升级只把费用 1 -> 0，效果不变。
// 实现：力量用内置 StrengthPower（负值即失去力量）；降费走 base.EnergyCost.UpgradeBy(-1)（同内置卡 Abundance）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class JieDaoShaRenCard : SGSModV3BaseCard
{
    private const string StrengthGainKey = "StrengthGain";
    private const string StrengthLossKey = "StrengthLoss";

    public JieDaoShaRenCard() : base(2, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new DynamicVar(StrengthGainKey, 1m),
        new DynamicVar(StrengthLossKey, 1m)
    };

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int gain = base.DynamicVars[StrengthGainKey].IntValue;
        int loss = base.DynamicVars[StrengthLossKey].IntValue;

        // 自己获得力量
        PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, gain, Owner.Creature, this, false);
        // 敌人失去力量（负值）
        PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Target, -loss, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 数据表：升级只降费（1 -> 0），效果数值不变
        base.EnergyCost.UpgradeBy(-1);
    }
}
