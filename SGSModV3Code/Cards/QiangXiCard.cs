using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SGSModV3.Characters;
using SGSModV3;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 强袭：攻击，费 1，品质蓝。失去 3 点生命，造成 15 点伤害。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class QiangXiCard : SGSModV3BaseCard
{
    private const string HpLossKey = "HpLoss";

    public QiangXiCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new DynamicVar(HpLossKey, 3m),
        new DamageVar(15m, ValueProp.Move)
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int hpLoss = base.DynamicVars[HpLossKey].IntValue;
        // 先失去生命。
        await CreatureCmd.Damage(choiceContext, Owner.Creature, hpLoss, ValueProp.Unblockable, Owner.Creature, this, cardPlay);
        // 再对目标造成伤害。
        await SGSModV3Damage.DealPhysicalDamage(choiceContext, cardPlay.Target, DynamicVars.Damage, Owner.Creature, Owner, this, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }
}
