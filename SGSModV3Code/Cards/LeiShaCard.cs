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

// 雷杀：造成 4 点雷电伤害，触发两次。升级后 6 点触发两次。
// 统一走 SGSModV3Damage.DealThunderDamage，自动联动驭雳/铁索连环，并贴雷电视觉标签。
[RegisterCard(typeof(SGSModV3CardPool))]
[RegisterCharacterStarterCard(typeof(SGSModV3Character), 1)]
public sealed class LeiShaCard : SGSModV3BaseCard
{
    public LeiShaCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 触发两次：每次单独结算。
        await SGSModV3Damage.DealThunderDamage(choiceContext, cardPlay.Target!, DynamicVars.Damage, Owner.Creature, Owner, this, cardPlay);
        await SGSModV3Damage.DealThunderDamage(choiceContext, cardPlay.Target!, DynamicVars.Damage, Owner.Creature, Owner, this, cardPlay);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}
