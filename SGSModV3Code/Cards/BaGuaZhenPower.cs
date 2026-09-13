using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SGSModV3.Cards;

// 八卦阵：受到的所有伤害减少 50%。
// 参考实现：遗物「钻石头冠」（减伤类遗物走的是 ModifyDamage 系列钩子）。
//   本 MOD 里没有对应内置 Power，所以自建：
//   在 ModifyDamageMultiplicative 里判断「挨打的是不是自己」，是就乘 0.5。
//   该钩子对伤害结算与卡面预览都会调用，所以鼠标悬停时数字也是减半后的。
[RegisterPower]
public sealed class BaGuaZhenPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageMultiplicative(Creature target, decimal amount, ValueProp props, Creature dealer, CardModel cardSource, CardPlay cardPlay)
    {
        if (target != base.Owner)
        {
            return 1m;
        }
        return 0.5m;
    }
}
