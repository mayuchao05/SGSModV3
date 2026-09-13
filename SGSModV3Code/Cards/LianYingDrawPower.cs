using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 连营：当你失去最后 1 张牌后（手牌变空），摸 1 张牌。
// 参考实现：遗物「不休陀螺 UnceasingTop」——用的就是 AfterHandEmptied 这个钩子。
//   官方还给了一个阶段保护：不要在「起手摸牌之前」和「回合结束手牌清空之后」触发，
//   否则自动出牌的瞬间手牌必然为空，会白送一张。
//   这里用等价的保险：只在战斗进行中、且确实是本人在出牌阶段时触发。
[RegisterPower]
public sealed class LianYingDrawPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterHandEmptied(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != base.Owner.Player)
        {
            return;
        }
        // 起手摸牌前 / 回合结束手牌清空后不触发（对应官方 IsValidPhase 的保护）。
        if (base.Owner.Player?.PlayerCombatState == null)
        {
            return;
        }
        // 只在自动/手动出牌阶段触发，避免回合开始摸牌前、回合结束弃牌后的空窗期误触发。
        PlayerTurnPhase phase = base.Owner.Player.PlayerCombatState.Phase;
        if (phase - PlayerTurnPhase.AutoPrePlay > 2)
        {
            return;
        }

        await CardPileCmd.Draw(choiceContext, base.Amount, player, false);
    }
}
