using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SGSModV3.Characters;
using SGSModV3;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 琴音：攻击，费 1，品质蓝。失去 2 点生命，对所有敌人造成 12 点伤害。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class QinYinCard : SGSModV3BaseCard
{
    private const string HpLossKey = "HpLoss";

    public QinYinCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new DynamicVar(HpLossKey, 2m),
        new DamageVar(12m, ValueProp.Move)
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int hpLoss = base.DynamicVars[HpLossKey].IntValue;
        // 先失去生命。
        await CreatureCmd.Damage(choiceContext, Owner.Creature, hpLoss, ValueProp.Unblockable, Owner.Creature, this, cardPlay);
        // 对所有敌人造成伤害。
        IEnumerable<Creature> targets = this.GetTargets(Owner.Creature);
        await SGSModV3Damage.DealPhysicalDamageToTargets(choiceContext, targets, DynamicVars.Damage, Owner.Creature, Owner, this, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
