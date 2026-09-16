using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 龙怒：能力，费 2，品质金。回合开始时失去 3 点生命，获得 1 点能量；升级后获得 2 点。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class LongNuCard : SGSModV3BaseCard
{
    public LongNuCard() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new EnergyVar(2)
    };

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        PowerCmd.Apply<LongNuPower>(choiceContext, Owner.Creature, DynamicVars.Energy.BaseValue, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}
