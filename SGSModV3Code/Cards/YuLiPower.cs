using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SGSModV3.Cards;

/// <summary>
/// 驭雳：你造成非自损伤害时，非雷电伤害统一转为雷电伤害；本身就是雷电伤害则伤害 +2。
/// 逻辑在 SGSModV3Damage.DealDamage 中统一处理。
/// </summary>
[RegisterPower]
public sealed class YuLiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");
}
