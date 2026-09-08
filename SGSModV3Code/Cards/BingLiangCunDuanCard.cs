using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 兵粮寸断：技能，费1，升级后费0。
// 效果：使一名敌人获得 1 层虚弱与 1 层易伤。
// 机制参考杀戮尖塔2内置的减益能力，直接用原生的 WeakPower / VulnerablePower。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class BingLiangCunDuanCard : SGSModV3BaseCard
{
    public BingLiangCunDuanCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature target = cardPlay.Target!;
        // 敌人获得 1 层虚弱（攻击伤害降低）
        PowerCmd.Apply<WeakPower>(choiceContext, target, 1m, Owner.Creature, this, false);
        // 敌人获得 1 层易伤（受到伤害提升）
        PowerCmd.Apply<VulnerablePower>(choiceContext, target, 1m, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 升级：费用 1 -> 0
        this.EnergyCost?.UpgradeBy(-1);
    }
}
