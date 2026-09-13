using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 白银狮子：下回合开始时回复 N 点血量（一次性）。
// 机制仿照内置 DrawCardsNextTurnPower（下回合抽牌）的 AfterSideTurnStart 钩子：
// 下个己方回合开始时回血，然后立刻把自己移除，保证只触发一次。
// 注：内置 RegenPower 是「回合结束前」回血（时机早一拍），不符合数据表「下回合开始时」的要求。
// 注2：CreatureCmd.Heal 有不需要 PlayerChoiceContext 的重载，正好适配本钩子（该钩子不提供 ctx）。
[RegisterPower]
public sealed class BaiYinShiZiHealPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Owner == null)
        {
            return;
        }
        // 只在自己这一侧的回合开始时触发
        if (side != Owner.Side)
        {
            return;
        }
        if (!participants.Contains(Owner))
        {
            return;
        }

        await CreatureCmd.Heal(Owner, base.Amount, true);
        // 一次性，回完就移除
        await PowerCmd.Remove(this);
    }
}
