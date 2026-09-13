using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Relics;

// 混毒弯匕的「临时力量」记账状态。
//
// 为什么不用游戏内置的 TemporaryStrengthPower：
//   PowerCmd.Apply<TemporaryStrengthPower>() 会抛
//   KeyNotFoundException: The given key 'POWER.TEMPORARY_STRENGTH_POWER' was not present in the dictionary
//   —— 该 Power 类虽然存在于 sts2.dll，却没有被注册进 ModelDb，无法用泛型 Apply 取到。
//   （它内部其实也是转手 Apply StrengthPower，再在回合末收回。）
//
// 所以这里照搬它的思路，但用我们自己已注册的 Power：
//   1. 遗物每打出一张攻击牌 → 加 1 层本 Power（记账），同时直接 Apply<StrengthPower>(+1) 真正加力量；
//   2. 本侧回合结束 → 按本 Power 的层数 Apply<StrengthPower>(-层数) 收回力量，再移除自己。
// StrengthPower 是本 MOD 已在用的（借刀杀人、南蛮入侵），确认注册正常、且支持负值。
[RegisterPower]
public sealed class HunDuWanBiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png",
            $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // 只在自己这一侧回合结束时结算。
        if (side != base.Owner.Side)
        {
            return;
        }

        int tempStrength = base.Amount;
        if (tempStrength > 0)
        {
            // 收回本回合临时获得的力量（等量负值）。
            await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, -tempStrength, base.Owner, null, true);
        }

        await PowerCmd.Remove(this);
    }
}
