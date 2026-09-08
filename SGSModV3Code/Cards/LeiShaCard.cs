using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 雷杀：造成 4 点伤害，触发两次（雷电）。升级后 6 点触发两次。
// 简单结算卡：调用两次 CreatureCmd.Damage。雷电伤害类型暂以物理数值表达。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class LeiShaCard : SGSModV3BaseCard
{
    public LeiShaCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, false)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, ValueProp.Move)
    ];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 触发两次：每次单独结算，便于后续接入雷电伤害钩子。
        CreatureCmd.Damage(choiceContext, new Creature[] { cardPlay.Target! }, DynamicVars.Damage, Owner.Creature, this, cardPlay);
        CreatureCmd.Damage(choiceContext, new Creature[] { cardPlay.Target! }, DynamicVars.Damage, Owner.Creature, this, cardPlay);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}
