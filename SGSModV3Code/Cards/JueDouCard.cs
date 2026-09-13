using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SGSModV3.Characters;
using SGSModV3;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 决斗：攻击，费1，品质蓝，单体敌人。
// 数据表：造成 4 点伤害，你手上每有 1 张攻击牌伤害加 2（升级后 6 点，每张 +3）。
// 参考实现：铁甲战士「跃跃欲试」（伤害随手上攻击牌数量成长的攻击牌）。
//   做法：结算时现算「手牌里攻击牌的张数 × 每张加成」，直接把算好的数值交给
//   CreatureCmd.Damage 的 decimal 重载 —— 不去改 DynamicVars.Damage.BaseValue，
//   否则会像「爪击 Claw」那样把加成永久累加到这张卡上（打一次涨一次）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class JueDouCard : SGSModV3BaseCard
{
    private const string BonusKey = "Bonus";

    public JueDouCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new DamageVar(4m, ValueProp.Move),
        new DynamicVar(BonusKey, 2m)
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int attackCardsInHand = Owner.PlayerCombatState?.Hand.Cards
            .Count(c => c.Type == CardType.Attack) ?? 0;

        decimal perCard = base.DynamicVars[BonusKey].BaseValue;
        decimal total = base.DynamicVars.Damage.BaseValue + attackCardsInHand * perCard;

        await SGSModV3Damage.DealPhysicalDamage(choiceContext, cardPlay.Target, new DamageVar(total, ValueProp.Move), Owner.Creature, Owner, this, cardPlay);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);      // 4 -> 6
        base.DynamicVars[BonusKey].UpgradeValueBy(1m);   // 2 -> 3
    }
}
