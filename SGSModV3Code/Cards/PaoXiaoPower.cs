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
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Type != CardType.Attack)
        {
            return false;
        }
        modifiedCost = System.Math.Max(0m, originalCost - base.Amount);
        return true;
    }
}
