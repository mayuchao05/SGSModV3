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

// 寂灭（序号48）：能力，费2，金。
// 回合结束时若你本回合造成了至少16点雷电伤害，对一名敌人造成30点雷电伤害（升级24/45）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class JiMieCard : SGSModV3BaseCard
{
    public JiMieCard() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 卡面数值必须是 DynamicVar，否则升级后描述的硬编码数字不会变。
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new DynamicVar("Threshold", 16m),
        new DamageVar(30m, ValueProp.Move)
    };

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Amount=1 未升级，Amount=2 升级；Power 内部据此切换阈值与伤害。
        PowerCmd.Apply<JiMiePower>(choiceContext, Owner.Creature, IsUpgraded ? 2m : 1m, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Threshold"].UpgradeValueBy(8m);   // 16 -> 24
        DynamicVars.Damage.UpgradeValueBy(15m);        // 30 -> 45
    }
}
