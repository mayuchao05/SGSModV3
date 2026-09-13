using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 儒贤触发状态：打出儒贤的本回合内，每打出1张牌（排除儒贤自身）后摸1张。
// 升级版额外在打出儒贤时获得1点能量（由 RuXianCard.OnPlay 处理）。
// 效果持续到「本回合」结束：玩家回合结束（BeforeSideTurnEnd）时移除自身。
[RegisterPower]
public sealed class RuXianPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 排除儒贤自身：它不算「之后打出的牌」。
        if (cardPlay.Card is RuXianCard)
        {
            return;
        }
        if (cardPlay.Card.Owner != base.Owner.Player)
        {
            return;
        }
        // 本回合内每张牌都触发，不移除自身。
        await CardPileCmd.Draw(choiceContext, 1m, base.Owner.Player, false);
    }

    // 玩家本回合结束时移除，实现「本回合内持续生效」语义。
    public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != base.Owner.Side)
        {
            return Task.CompletedTask;
        }
        return PowerCmd.Remove(this);
    }
}
