using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SGSModV3.Cards;

// 英姿：每回合开始时额外摸 N 张牌（永久持续整场战斗）。
// 为什么不用内置 ClarityPower（明晰）：
//   ClarityPower 的 Amount 是「剩余回合数」，Modifier 固定 +1。
//   打出两张英姿会变成「接下来 198 个回合，每回合 +1」，而不是期望的「每回合 +2」。
// 本状态改成就绪的改写：Amount = 每回合额外抽牌数，且不复写 AfterSideTurnStart
//   （内置 ClarityPower / DrawCardsNextTurnPower 就是在这个钩子里把自己扣完/移除的），
//   因此层数不会随时间衰减，整场战斗一直生效。
//   → 打出 2 张英姿 = 2 层 = 每回合额外摸 2 张，符合数据表期望。
[RegisterPower]
public sealed class YingZiDrawPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != base.Owner.Player)
        {
            return count;
        }
        return count + base.Amount;
    }
}
