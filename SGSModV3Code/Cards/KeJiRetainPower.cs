using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 克己：回合结束时，若你本回合没有打出过攻击牌，则保留手牌。
// 参考实现：
//   · 遗物「孙子兵法 ArtOfWar」——"本回合是否打出过攻击牌 → 回合结束结算"的思路。
//   · 遗物「三角铃鼓 Tingsha」——同样是「本回合累计条件 → 回合结束结算」的思路。
//   保留手牌本身用官方自带的 CardCmd.ApplySingleTurnRetain（给单张牌加一回合的「保留」）。
//
// ⚠️ 关键修正：是否「本回合打出过攻击牌」必须查战斗历史，不能靠自身标记。
//   克己是中途打出的能力牌，它挂上 Power 之前那回合里打的攻击牌不会被 AfterCardPlayed 捕获，
//   会导致「前面打了攻击、最后才打克己」时仍误保留手牌。
[RegisterPower]
public sealed class KeJiRetainPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override PowerStackType StackType => PowerStackType.Counter;

    // 要在手牌被弃掉「之前」打上保留，所以用 BeforeSideTurnEnd（AfterSideTurnEnd 时手牌已经清空了）。
    public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != base.Owner.Side)
        {
            return Task.CompletedTask;
        }
        if (!participants.Contains(base.Owner))
        {
            return Task.CompletedTask;
        }

        ICombatState? combatState = base.Owner.CombatState;
        if (combatState == null)
        {
            return Task.CompletedTask;
        }

        // 本回合（本侧、当前回合）owner 是否打出过攻击牌——直接查战斗历史，
        // 这样即使攻击牌是在克己打出之前就打出的，也能正确判定。
        bool attackedThisTurn = CombatManager.Instance.History.CardPlaysFinished
            .Any(entry => entry.HappenedThisTurn(combatState)
                          && entry.CardPlay.Card?.Owner?.Creature == base.Owner
                          && entry.CardPlay.Card.Type == CardType.Attack);
        if (attackedThisTurn)
        {
            return Task.CompletedTask;
        }

        IReadOnlyList<CardModel> hand = base.Owner.Player?.PlayerCombatState?.Hand.Cards
                                        ?? new List<CardModel>();
        foreach (CardModel card in hand.ToList())
        {
            CardCmd.ApplySingleTurnRetain(card);
        }
        return Task.CompletedTask;
    }
}
