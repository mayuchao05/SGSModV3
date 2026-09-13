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

// 闪电（序号11）：能力，费 1，品质蓝。
// 回合开始时你失去 1 点生命，一名随机敌人受到 {Damage} 点雷电伤害；升级后 12 点。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class ShanDianCard : SGSModV3BaseCard
{
    public ShanDianCard() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9m, ValueProp.Move)
    ];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Amount = 雷电伤害值（9 / 升级 12），Power 内读取。
        PowerCmd.Apply<ShanDianPower>(choiceContext, Owner.Creature, DynamicVars.Damage.BaseValue, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
