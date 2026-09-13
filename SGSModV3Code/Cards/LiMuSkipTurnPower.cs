using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 立牧的“跳过下个回合”状态：一次性。
// 在下个玩家回合开始（AfterSideTurnStart）时，立即让玩家结束回合，随后自移除。
// 之所以用结束回合而非 Stun：Stun 只对敌人有效（玩家 Monster==null 会抛异常），
// 而对玩家“失去回合”的标准做法就是在其回合开始阶段直接结束回合。
[RegisterPower]
public sealed class LiMuSkipTurnPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        // 仅在本玩家这一侧回合开始时触发（敌人回合不处理）。
        if (side != base.Owner.Side)
        {
            return;
        }

        // 先移除自身，避免本回合再次触发。
        await PowerCmd.Remove(this);

        // 立即结束玩家回合，实现“跳过本回合”。
        PlayerCmd.EndTurn(base.Owner.Player, false);
    }
}
