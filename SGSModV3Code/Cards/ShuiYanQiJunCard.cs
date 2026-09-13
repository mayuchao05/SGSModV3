using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 水淹七军：攻击，费2，品质蓝。
// 数据表：造成10点雷电伤害，使目标获得2层虚弱与2层易伤（升级15点、3层）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class ShuiYanQiJunCard : SGSModV3BaseCard
{
    public ShuiYanQiJunCard() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SGSModV3Damage.DealThunderDamage(choiceContext, cardPlay.Target!, DynamicVars.Damage, Owner.Creature, Owner, this, cardPlay);

        PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target!, 2m, Owner.Creature, this, false);
        PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target!, 2m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5m);
}
