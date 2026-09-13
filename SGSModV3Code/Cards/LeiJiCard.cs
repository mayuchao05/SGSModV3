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

// 雷击（序号27）：能力，费2（升级费2不变），蓝。
// 你每格挡1次攻击，就对1名随机敌人造成3点雷电伤害（升级5点）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class LeiJiCard : SGSModV3BaseCard
{
    public LeiJiCard() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3m, ValueProp.Move)
    ];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        PowerCmd.Apply<LeiJiPower>(choiceContext, Owner.Creature, DynamicVars.Damage.BaseValue, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}
