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

// 万箭齐发：对全部敌人造成伤害（AOE）。
// 关键：全体敌人目标下 cardPlay.Target 为 null，框架不会自动结算伤害，
// 必须用 GetTargets 解析全体敌人后显式调用 CreatureCmd.Damage。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class WanJianQiFaCard : SGSModV3BaseCard
{
    public WanJianQiFaCard() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 解析这张牌（AllEnemies 目标类型）对应的全体敌人。
        IEnumerable<Creature> targets = this.GetTargets(Owner.Creature);

        // 通过战斗命令对全体敌人结算伤害（走正常的伤害/格挡/死亡/动画流程）。
        await SGSModV3Damage.DealPhysicalDamageToTargets(choiceContext, targets, DynamicVars.Damage, Owner.Creature, Owner, this, cardPlay);

        return;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }
}
