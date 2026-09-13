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

// 诈降：技能，费 0，品质蓝。失去 3 点生命，获得 2 点能量。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class ZhaXiangCard : SGSModV3BaseCard
{
    private const string HpLossKey = "HpLoss";

    public ZhaXiangCard() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new DynamicVar(HpLossKey, 3m),
        new EnergyVar(2)
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int hpLoss = base.DynamicVars[HpLossKey].IntValue;
        // 失去生命：对自己造成无视护甲的伤害。
        await CreatureCmd.Damage(choiceContext, Owner.Creature, hpLoss, ValueProp.Unblockable, Owner.Creature, this, cardPlay);
        // 获得能量。
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
    }
}
