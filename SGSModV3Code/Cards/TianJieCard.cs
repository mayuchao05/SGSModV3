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

// 天劫（序号49）：能力，费2，蓝。
// 每回合打出 5 张牌时，对随机敌人造成 4 点雷电伤害 5 次（升级 6 点）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class TianJieCard : SGSModV3BaseCard
{
    public TianJieCard() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, ValueProp.Move)
    ];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Amount=1 未升级，Amount=2 升级；Power 内部据此切换每次伤害。
        PowerCmd.Apply<TianJiePower>(choiceContext, Owner.Creature, IsUpgraded ? 2m : 1m, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}
