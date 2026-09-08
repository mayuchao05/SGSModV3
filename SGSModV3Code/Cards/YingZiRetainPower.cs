using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;

namespace SGSModV3.Cards;

// 英姿（升级）效果：每个玩家回合结束时，令手牌中的一张牌获得“单回合保留”，
// 使其在下个回合开始时仍留在手中。
// 实现为挂在玩家身上的临时能力，持续整场战斗（LastForXExtraTurns 设大值）。
// OriginModel / InternallyAppliedPower 用 StrengthPower 作占位（amount=0 无实际效果），
// 真正的保留逻辑在 AfterSideTurnEnd 钩子里完成。
[RegisterPower]
public sealed class YingZiRetainPower : ModTemporaryPowerTemplate
{
    // 占位内部能力：0 点力量，无任何实际效果，仅用于满足临时能力的“包装”要求。
    public override PowerModel InternallyAppliedPower => new StrengthPower();

    // 用于解析标题/悬停提示的来源模型占位（基类要求返回 AbstractModel）。
    public override AbstractModel OriginModel => new StrengthPower();

    protected override bool IsPositive => true;

    // 持续整场战斗（覆盖典型战斗回合数）。
    protected override int LastForXExtraTurns => 999;

    public override Task AfterSideTurnEnd(PlayerChoiceContext ctx, CombatSide side, IEnumerable<Creature> creatures)
    {
        // 只在"玩家这一侧"回合结束时触发（能力挂在玩家战斗体上）。
        if (Owner == null || !creatures.Contains(Owner))
            return Task.CompletedTask;

        // 取玩家手牌：战斗状态的 Hand 牌堆，其 .Cards 为手牌集合。
        if (Owner.CombatState is not PlayerCombatState pcs)
            return Task.CompletedTask;
        var hand = pcs.Hand?.Cards;
        if (hand == null)
            return Task.CompletedTask;

        // 取手牌中第一张牌，给它单回合保留（重复保留无害）。
        var card = hand.FirstOrDefault(c => c != null);
        if (card != null)
            CardCmd.ApplySingleTurnRetain(card);
        return Task.CompletedTask;
    }
}
