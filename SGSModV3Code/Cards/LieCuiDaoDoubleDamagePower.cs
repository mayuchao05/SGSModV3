using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SGSModV3.Cards;

// 烈淬刀之力：每回合你的第 1 次伤害翻倍，之后不再翻倍，回合开始复位。
//
// 设计要点（沿用酒 JiuDoubleDamagePower 的经验）：
//   本 MOD 的攻击牌直接调 CreatureCmd.Damage，不经过 AttackCommand，
//   所以 BeforeAttack / AfterAttack 这套钩子永远不触发，必须用 ModifyDamageMultiplicative + cardPlay 令牌：
//     - cardPlay == null  -> 预览/非卡牌伤害：不翻倍、也不消耗次数。
//     - consumedBy == null -> 本回合第一次真实伤害：翻倍并记录 cardPlay。
//     - consumedBy == cardPlay（同一张牌的多次伤害/多目标）-> 继续翻倍。
//     - 其他 -> 本回合已用过，返回 1。
//   复位时机：本侧回合开始（AfterSideTurnStart）把 consumedBy 清空。
[RegisterPower]
public sealed class LieCuiDaoDoubleDamagePower : ModPowerTemplate
{
    private sealed class Data
    {
        // 本回合已完成翻倍的那次卡牌打出；null 表示本回合还没用过。
        public CardPlay? consumedBy;
    }

    protected override object InitInternalData() => new Data();

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override decimal ModifyDamageMultiplicative(Creature target, decimal amount, ValueProp props,
        Creature dealer, CardModel cardSource, CardPlay cardPlay)
    {
        // 只作用于本玩家对自己以外的目标造成的、来自自己卡牌的伤害。
        if (dealer != base.Owner || target == base.Owner)
        {
            return 1m;
        }

        if (cardSource == null || cardSource.Owner?.Creature != base.Owner)
        {
            return 1m;
        }

        // 预览或非卡牌来源：不翻倍、也不消耗本回合次数。
        if (cardPlay == null)
        {
            return 1m;
        }

        Data data = base.GetInternalData<Data>();

        if (data.consumedBy == null)
        {
            data.consumedBy = cardPlay;
            return 2m;
        }

        if (data.consumedBy == cardPlay)
        {
            return 2m;
        }

        return 1m;
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != base.Owner.Side)
        {
            return Task.CompletedTask;
        }

        base.GetInternalData<Data>().consumedBy = null;
        return Task.CompletedTask;
    }
}
