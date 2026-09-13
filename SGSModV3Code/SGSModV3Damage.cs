using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using SGSModV3.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGSModV3;

/// <summary>
/// 元素伤害类型标记。
/// 游戏原生没有火焰/雷电概念，所有属性伤害逻辑在 MOD 层集中处理。
/// </summary>
public enum ElementalDamageType
{
    Fire,
    Thunder,
    Physical // 普通非元素伤害，仅在驭雳转换前使用。
}

/// <summary>
/// MOD 自定义属性伤害分发器。
/// 集中处理：融火（火焰翻倍）、驭雳（非雷电伤害转雷电；雷电伤害 +2）、铁索连环（属性伤害 AOE）、
/// 元素视觉标签、回合雷电伤害累计（供寂灭读取）。
/// </summary>
public static class SGSModV3Damage
{
    // 每玩家每回合的雷电伤害累计（原始雷电伤害，供寂灭读取）。
    private static readonly Dictionary<Player, decimal> _thunderDamageThisTurn = new();

    public static decimal GetThunderDamageThisTurn(Player player)
    {
        return _thunderDamageThisTurn.TryGetValue(player, out decimal value) ? value : 0m;
    }

    public static void ResetThunderDamageThisTurn(Player player)
    {
        _thunderDamageThisTurn.Remove(player);
    }

    public static void AddThunderDamageThisTurn(Player player, decimal amount)
    {
        if (amount <= 0m)
            return;
        if (!_thunderDamageThisTurn.TryGetValue(player, out decimal current))
            current = 0m;
        _thunderDamageThisTurn[player] = current + amount;
    }

    /// <summary>
    /// 通用元素伤害入口。MOD 内所有非自损玩家伤害都应优先走这里，
    /// 以便统一处理融火、驭雳、铁索连环与视觉标签。
    /// card/cardPlay 可为 null（例如由 Power 触发伤害时），内部会回退到单体重载。
    /// </summary>
    public static async Task DealDamage(
        PlayerChoiceContext ctx,
        Creature target,
        DamageVar damageVar,
        Creature dealer,
        Player dealerPlayer,
        CardModel? card,
        CardPlay? cardPlay,
        ElementalDamageType element)
    {
        // 自损不走元素转换。
        bool isSelfDamage = dealer == target;

        // 驭雳：非雷电伤害统一转为雷电；雷电伤害 +Amount*2。
        ApplyModifiers(dealer, ref element, ref damageVar, isSelfDamage);

        // 铁索连环：属性伤害命中所有敌人（普通物理伤害保持单体）。
        IEnumerable<Creature> targets = ResolveElementalTargets(dealer, target, element);
        var targetList = targets.ToList();

        // 攻击钩子：花园曼等敌人“受击加护甲”的反应依赖 AttackCommand 流程
        // （Hook.BeforeAttack / AfterAttack）。我们直接走 CreatureCmd.Damage，必须手动补上
        // 这两个钩子，否则敌人的受击反应（如加护甲、荆棘）不会触发。
        // 自损（dealer == target）不算攻击，不触发；无 card source 时也无法构成攻击。
        var combatState = dealer.CombatState;
        AttackCommand? attackCmd = null;
        if (card != null && cardPlay != null && combatState != null && !isSelfDamage)
        {
            attackCmd = new AttackCommand(damageVar.BaseValue).FromCard(card, cardPlay);
            attackCmd = targetList.Count == 1
                ? attackCmd.Targeting(targetList[0])
                : attackCmd.TargetingAllOpponents(combatState);
            await Hook.BeforeAttack(combatState, attackCmd);
        }

        // 执行伤害。优先用单体重载（不需要 card source）；
        // 只有铁索连环导致多目标且存在 card source 时才用 AOE 重载。
        if (targetList.Count == 1)
        {
            if (card != null && cardPlay != null)
                await CreatureCmd.Damage(ctx, targetList, damageVar, dealer, card, cardPlay);
            else
                await CreatureCmd.Damage(ctx, targetList[0], damageVar, dealer);
        }
        else if (card != null)
        {
            await CreatureCmd.Damage(ctx, targetList, damageVar, dealer, card, cardPlay!);
        }
        else
        {
            // Power 触发且触发铁索连环：没有 card source，逐个用单体结算。
            foreach (Creature t in targetList)
            {
                if (t.IsAlive && t.IsHittable)
                    await CreatureCmd.Damage(ctx, t, damageVar, dealer);
            }
        }

        if (attackCmd != null && combatState != null)
            await Hook.AfterAttack(combatState, ctx, attackCmd);

        // 视觉标签。
        if (card != null)
        {
            if (element == ElementalDamageType.Fire)
            {
                ApplyElementVisualTag<FireDamageVisualPower>(ctx, targetList, dealer, card);
            }
            else if (element == ElementalDamageType.Thunder)
            {
                ApplyElementVisualTag<ThunderDamageVisualPower>(ctx, targetList, dealer, card);
            }
        }

        // 累计原始雷电伤害（供寂灭）。
        if (element == ElementalDamageType.Thunder)
        {
            AddThunderDamageThisTurn(dealerPlayer, damageVar.BaseValue * targetList.Count);
        }
    }

    /// <summary>
    /// 造成火焰伤害（兼容旧调用）。
    /// </summary>
    public static Task DealFireDamage(PlayerChoiceContext ctx, Creature target, DamageVar damageVar, Creature dealer, Player dealerPlayer, CardModel card, CardPlay cardPlay)
    {
        return DealDamage(ctx, target, damageVar, dealer, dealerPlayer, card, cardPlay, ElementalDamageType.Fire);
    }

    /// <summary>
    /// 造成雷电伤害（兼容旧调用）。
    /// </summary>
    public static Task DealThunderDamage(PlayerChoiceContext ctx, Creature target, DamageVar damageVar, Creature dealer, Player dealerPlayer, CardModel card, CardPlay cardPlay)
    {
        return DealDamage(ctx, target, damageVar, dealer, dealerPlayer, card, cardPlay, ElementalDamageType.Thunder);
    }

    /// <summary>
    /// Power 等无 card source 的来源造成雷电伤害。
    /// </summary>
    public static Task DealThunderDamage(PlayerChoiceContext ctx, Creature target, DamageVar damageVar, Creature dealer, Player dealerPlayer)
    {
        return DealDamage(ctx, target, damageVar, dealer, dealerPlayer, null, null, ElementalDamageType.Thunder);
    }

    /// <summary>
    /// 造成普通物理伤害。可被驭雳转为雷电，但不受融火/铁索连环影响。
    /// </summary>
    public static Task DealPhysicalDamage(PlayerChoiceContext ctx, Creature target, DamageVar damageVar, Creature dealer, Player dealerPlayer, CardModel card, CardPlay cardPlay)
    {
        return DealDamage(ctx, target, damageVar, dealer, dealerPlayer, card, cardPlay, ElementalDamageType.Physical);
    }

    /// <summary>
    /// 驭雳/融火：属性伤害加成与转换。
    /// </summary>
    private static void ApplyModifiers(Creature dealer, ref ElementalDamageType element, ref DamageVar damageVar, bool isSelfDamage)
    {
        if (!isSelfDamage)
        {
            var yuLi = dealer.GetPower<YuLiPower>();
            if (yuLi != null)
            {
                if (element != ElementalDamageType.Thunder)
                {
                    element = ElementalDamageType.Thunder;
                }
                else
                {
                    damageVar = new DamageVar(damageVar.BaseValue + yuLi.Amount * 2m, damageVar.Props);
                }
            }
        }

        if (element == ElementalDamageType.Fire && dealer.GetPower<RongHuoPower>() != null)
        {
            damageVar = new DamageVar(damageVar.BaseValue * 2m, damageVar.Props);
        }
    }

    public static async Task DealDamageToTargets(
        PlayerChoiceContext ctx,
        IEnumerable<Creature> targets,
        DamageVar damageVar,
        Creature dealer,
        Player dealerPlayer,
        CardModel? card,
        CardPlay? cardPlay,
        ElementalDamageType element)
    {
        var list = targets.Where(t => t != null && t.IsAlive && t.IsHittable).ToList();

        // 攻击钩子（同 DealDamage）：让敌人的受击反应（加护甲/荆棘等）随自定义多目标伤害触发。
        var combatState = dealer.CombatState;
        AttackCommand? attackCmd = null;
        if (card != null && cardPlay != null && combatState != null)
        {
            attackCmd = new AttackCommand(damageVar.BaseValue).FromCard(card, cardPlay).TargetingAllOpponents(combatState);
            await SafeRaiseBeforeAttack(combatState, attackCmd);
        }

        foreach (Creature t in list)
        {
            ElementalDamageType el = element;
            DamageVar dv = damageVar;
            ApplyModifiers(dealer, ref el, ref dv, dealer == t);
            // 带上 card/cardPlay，让伤害被识别为攻击，触发敌人的受击反应（如花园曼加护甲）。
            if (card != null && cardPlay != null)
                await CreatureCmd.Damage(ctx, t, dv, dealer, card, cardPlay);
            else
                await CreatureCmd.Damage(ctx, t, dv, dealer);
            if (card != null)
            {
                if (el == ElementalDamageType.Fire)
                    ApplyElementVisualTag<FireDamageVisualPower>(ctx, new[] { t }, dealer, card);
                else if (el == ElementalDamageType.Thunder)
                    ApplyElementVisualTag<ThunderDamageVisualPower>(ctx, new[] { t }, dealer, card);
            }
            if (el == ElementalDamageType.Thunder)
                AddThunderDamageThisTurn(dealerPlayer, dv.BaseValue);
        }

        if (attackCmd != null && combatState != null)
            await Hook.AfterAttack(combatState, ctx, attackCmd);
    }

    public static Task DealPhysicalDamageToTargets(PlayerChoiceContext ctx, IEnumerable<Creature> targets, DamageVar damageVar, Creature dealer, Player dealerPlayer, CardModel card, CardPlay cardPlay)
    {
        return DealDamageToTargets(ctx, targets, damageVar, dealer, dealerPlayer, card, cardPlay, ElementalDamageType.Physical);
    }

    private static IEnumerable<Creature> ResolveElementalTargets(Creature dealer, Creature originalTarget, ElementalDamageType element)
    {
        if (element != ElementalDamageType.Physical && dealer.GetPower<TieSuoLianHuanPower>() != null && dealer.CombatState != null)
        {
            return dealer.CombatState.Enemies.Where(e => e.IsAlive && e.IsHittable).ToList();
        }
        return new[] { originalTarget };
    }

    /// <summary>
    /// 给目标贴上元素视觉标签（仅视觉，无实际效果，会在目标回合结束时自动消失）。
    /// </summary>
    private static void ApplyElementVisualTag<T>(PlayerChoiceContext ctx, IEnumerable<Creature> targets, Creature dealer, CardModel card)
        where T : PowerModel
    {
        foreach (Creature t in targets)
        {
            if (t == null || !t.IsAlive)
                continue;
            try
            {
                PowerCmd.Apply<T>(ctx, t, 1m, dealer, card, false);
            }
            catch
            {
                // 视觉标签不应导致伤害流程中断。
            }
        }
    }

    /// <summary>
    /// 获取存活的可命中敌人列表。
    /// </summary>
    public static IReadOnlyList<Creature> GetAliveEnemies(Creature dealer)
    {
        if (dealer?.CombatState == null)
            return Array.Empty<Creature>();
        return dealer.CombatState.Enemies.Where(e => e.IsAlive && e.IsHittable).ToList();
    }

    /// <summary>
    /// 安全触发攻击前钩子。我们的自定义伤害绕过了原生 AttackCommand 执行流程，
    /// 这里手动补上 BeforeAttack 让受击反应（荆棘、反伤等）有机会生效。
    /// 用 try/catch 兜底：即便某个钩子订阅者因命令信息不全而抛异常，
    /// 也绝不让核心伤害流程中断（否则所有攻击牌都会失效）。
    /// 异常会写入游戏日志（godot.log）以便排查。
    /// </summary>
    private static async Task SafeRaiseBeforeAttack(ICombatState combatState, AttackCommand command)
    {
        try
        {
            await Hook.BeforeAttack(combatState, command);
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"[SGSModV3] BeforeAttack hook error (ignored): {ex}");
        }
    }

    /// <summary>
    /// 安全触发攻击后钩子。敌人“受击加护甲”等反应依赖 AfterAttack；
    /// 同样用 try/catch 兜底，确保异常不会打断伤害结算。
    /// </summary>
    private static async Task SafeRaiseAfterAttack(ICombatState combatState, PlayerChoiceContext ctx, AttackCommand command)
    {
        try
        {
            await Hook.AfterAttack(combatState, ctx, command);
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"[SGSModV3] AfterAttack hook error (ignored): {ex}");
        }
    }

    /// <summary>
    /// 从存活敌人中随机选择一个。
    /// </summary>
    public static Creature? GetRandomEnemy(Creature dealer)
    {
        var enemies = GetAliveEnemies(dealer);
        if (enemies.Count == 0)
            return null;
        return enemies[Random.Shared.Next(enemies.Count)];
    }
}
