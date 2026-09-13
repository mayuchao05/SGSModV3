using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SGSModV3.Relics;

// RegisterRelic 会把遗物注册进指定遗物池。
// RegisterCharacterStarterRelic 会把它作为 SGSModV3Character 的初始遗物。
[RegisterRelic(typeof(SGSModV3RelicPool))]
[RegisterCharacterStarterRelic(typeof(SGSModV3Character))]
public sealed class SGSModV3Relic : ModRelicTemplate
{
    // 稀有度：起点遗物用 Common（与模板工程 ReferenceMod 一致，游戏内由 RegisterCharacterStarterRelic 保证为角色初始遗物）。
    public override RelicRarity Rarity => RelicRarity.Common;

    // 遗物的数值。这里会替换本地化中的 {Gold}。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new GoldVar(10)
    ];

    // 图片资源统一放在 AssetProfile 里配置。
    public override RelicAssetProfile AssetProfile => new(
        // 小图标（原版 85x85）。
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        // 轮廓图标（原版 85x85）。
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        // 大图标（原版 256x256）。
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    // 每次战斗结束时，获得 10 金币。
    public override async Task AfterCombatEnd(CombatRoom room)
    {
        Flash();
        await PlayerCmd.GainGold(base.DynamicVars.Gold.IntValue, base.Owner);
    }
}
