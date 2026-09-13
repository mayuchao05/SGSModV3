using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace SGSModV3.Cards;

/// <summary>
/// 出牌时驱动角色动画。
///
/// 背景：本 MOD 所有攻击牌都是直接调 CreatureCmd.Damage 结算的，不走 AttackCommand，
/// 而 AttackCommand.FromCard() 才是本体里唯一会把攻击者动画设为 "Attack" 的入口 → 攻击动画时有时无。
/// 技能/能力牌则根本没有任何触发源，所以人物一动不动。
///
/// 解法：在 CardModel.OnEnqueuePlayVfx（出牌视觉钩子）里显式调用
/// NCreature.SetAnimationTrigger("Attack"/"Cast")。RitsuLib 已 patch 该方法，
/// 会映射到 ModCreatureVisualPlayback 的 cue 表，从而驱动我们的 AnimatedSprite2D。
/// </summary>
internal static class CardAnimTrigger
{
    public static void Play(CardModel card)
    {
        var creature = card.Owner?.Creature;
        if (creature is null)
            return;

        // Type 为 Attack 走挥剑，其余（Skill / Power）走施法。
        string trigger = card.Type == CardType.Attack ? "Attack" : "Cast";
        creature.GetCreatureNode()?.SetAnimationTrigger(trigger);
    }
}
