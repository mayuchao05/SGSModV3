using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 连破：能力，费1（升级仍1），品质蓝（Uncommon）。
// 数据表：1名敌人死亡后，获得3点能量、摸4张牌（升级：4能量、6牌）。
// 参考遗物 Gremlin Horn（地精之角）：敌人死亡时获得能量并抽牌。
// 实现：打出后挂常驻状态 LianPoPower，监听敌人死亡（AfterDeath）给予能量与抽牌。
[RegisterPower]
public sealed class LianPoPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override PowerStackType StackType => PowerStackType.Counter;

    // Energy/Draw 作为 DynamicVar 注册（初始 0），真实数值由 LianPoCard.OnPlay 在“每次出牌”时累加进本实例。
    // 注意：叠加第二张连破时 AfterApplied 不会再次触发（框架文档明确：owner 已有该状态仅改变 amount 时不调用），
    // 所以绝不能把数值设在 AfterApplied 里；必须在卡 OnPlay 里累加到 GetPower 取到的真实实例上。
    // 状态栏通过 SmartDescription 自动注入这两个 DynamicVar，因此显示的就是累加后的真实总量。
    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new IntVar("Energy", 0m),
        new IntVar("Draw", 0m),
    };

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        // 只响应“敌人”死亡（不是自己、也不是友方）。
        if (creature.Side == base.Owner.Side)
        {
            return;
        }
        // 直接读累加后的总量，不再乘 Amount（每层数值可能因升级而不同，乘法会算错）。
        decimal energy = base.DynamicVars["Energy"].BaseValue;
        decimal draw = base.DynamicVars["Draw"].BaseValue;
        await PlayerCmd.GainEnergy(energy, base.Owner.Player);
        await CardPileCmd.Draw(choiceContext, draw, base.Owner.Player, false);
    }
}
