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

// 火杀：造成 8 点伤害（火焰）。升级后 12 点。
// 简单结算卡：直接用 CreatureCmd.Damage。火焰伤害类型暂以物理数值表达，视觉标签后续补。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class HuoShaCard : SGSModV3BaseCard
{
    public HuoShaCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, false)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move)
    ];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CreatureCmd.Damage(choiceContext, new Creature[] { cardPlay.Target! }, DynamicVars.Damage, Owner.Creature, this, cardPlay);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}
