using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SGSModV3.Cards;

// 咆哮：常驻状态（参考张飞）。你的攻击牌能量消耗减 1（每层再减 1）。
// 实现：override TryModifyEnergyCostInCombat，对攻击牌降费；Counter 叠加后每层 -1。
[RegisterPower]
public sealed class PaoXiaoPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override PowerStackType StackType => PowerStackType.Counter;

    // 攻击牌（杀/火杀/雷杀/决斗等）能量消耗减 Amount；Counter 叠加后每层再 -1。
    // Math.Max 防止变为负数。框架会把所有 model 的修改链式累加，所以多张咆哮会叠加。
    //
    // 多人模式关键修复：框架会对战斗中每张被打出的卡、向“所有存活状态”询问降费，
    // 因此必须限定「卡牌拥有者 == 本状态拥有者」才生效，否则会误减其他玩家攻击牌费用，
    // 且各客户端算出的扣费不一致导致数据不同步。
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner?.Creature != base.Owner)
        {
            return false;
        }
        if (card.Type != CardType.Attack)
        {
            return false;
        }
        modifiedCost = System.Math.Max(0m, originalCost - base.Amount);
        return true;
    }
}
