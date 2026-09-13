using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 苦肉：你回合内每次失去生命时，摸 {Amount} 张牌并获得 1 点能量。
//
// 【修复记录 2026-09-13 第二次】
// 第一版用 BeforeSideTurnStart/AfterSideTurnStart/AfterPlayerTurnStart 三个钩子置 “自己回合” 标志位，
// 实测仍不触发（日志里 KuRouPower 挂上了 8 次，但 “触发” 日志 0 条）。
// 根因判断不再依赖任何回合钩子：改为在受伤瞬间直接读战斗状态的当前回合方
//   ICombatState.CurrentSide（Creature.CombatState / PowerModel.CombatState 均可拿到），
//   只有 CurrentSide == Owner.Side 时才触发。这是确定性的，不依赖钩子是否被回调。
// 另：入口加了无条件诊断日志，下次复测可直接从日志看出卡在哪一步。
[RegisterPower]
public sealed class KuRouPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override PowerStackType StackType => PowerStackType.Counter;

    // 当前是否轮到“我方”行动。拿不到战斗状态时宽松放行（宁可多触发，也不要像之前那样静默失效）。
    private bool IsOwnTurn()
    {
        ICombatState? state = base.CombatState ?? base.Owner?.CombatState;
        if (state == null)
        {
            return true;
        }
        return state.CurrentSide == base.Owner.Side || state.CurrentSide == CombatSide.None;
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
    {
        decimal unblocked = result?.UnblockedDamage ?? 0m;
        bool isTargetOwner = target != null && target == base.Owner;
        Entry.Logger.Info($"[KuRou] AfterDamageReceived 收到回调: isTargetOwner={isTargetOwner}, unblocked={unblocked}, side={(base.CombatState ?? base.Owner?.CombatState)?.CurrentSide.ToString() ?? "null"}, 当前层数={base.Amount}");

        if (!isTargetOwner)
        {
            return;
        }
        // 被护甲全挡或未实际掉血时不触发。
        if (unblocked <= 0)
        {
            return;
        }
        if (!IsOwnTurn())
        {
            Entry.Logger.Info($"[KuRou] 跳过：不在自己回合");
            return;
        }

        Player? player = base.Owner.Player;
        if (player == null)
        {
            Entry.Logger.Info($"[KuRou] 跳过：Owner.Player 为空");
            return;
        }

        Entry.Logger.Info($"[KuRou] 触发：失去 {unblocked} 点生命，摸 {base.Amount} 张牌并获得 1 点能量。");
        await CardPileCmd.Draw(choiceContext, base.Amount, player, false);
        await PlayerCmd.GainEnergy(1m, player);
    }
}
