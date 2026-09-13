using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Random;
using SGSModV3.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SGSModV3.Ancients;

// 先古之民：蒲元（三国杀铁匠）
// 出现条件：第二章（Hive，Act Index = 1）开始时有 50% 概率遇到。
//
// 入口登记：RegisterActAncient(typeof(Hive)) -> RitsuLib 的 RegisterScopedModel ->
//   PrimeOwnedType -> ModelDb.GetEntry 被 ModelDbModdedEntryPatch 覆写，
//   得到本体的公开 Entry：SGS_MOD_V3_EVENT_PU_YUAN_ANCIENT（Category 取 AncientEventModel 之上的 EventModel）。
//   本地化键前缀就以此为准。
//
// 资源：只需覆写 AssetProfile / AncientPresentationAssetProfile，
//   RitsuLib 的 Event*/Ancient* 系列 Patch 会自动接到游戏侧所有读取点（立绘、背景场景、地图图标、跑图历史图标）。
//   LayoutScenePath / VfxScenePath 留空表示沿用原版（Ancient 布局 = ancient_event_layout.tscn）。
[RegisterActAncient(typeof(Hive))]
public sealed class PuYuanAncient : ModAncientEventTemplate
{
    // 自定义背景场景：res://SGSModV3/scenes/ancients/puyuan_bg.tscn
    // 之前置空是因为担心 NEventRoom 冻结，现已查明消费方逻辑：
    //   NAncientEventLayout._Ready -> _ancientEvent.CreateBackgroundScene()
    //     -> AssetCache.GetScene(BackgroundScenePath).Instantiate<Control>()
    //     -> AddChildSafely(%AncientBgContainer, ...)
    // 只要场景根节点是 Control 即可，NAncientBgContainer 只做自身缩放、不遍历子节点。
    // 当初的 NullReferenceException 是“路径不存在 -> GetScene 返回 null -> 对 null 调 Instantiate”
    // 造成的，并非自定义场景本身不合法。该 tscn 已确认随 PCK 导出（.remap 形式）。
    // PuYuanBackgroundPatch 会在该路径存在时放行，并在缺失时回退到原版 Hive 先古背景兜底。
    public override EventAssetProfile AssetProfile => new(
        LayoutScenePath: string.Empty,
        InitialPortraitPath: $"{Entry.ResPath}/images/ancients/puyuan_art.png",
        BackgroundScenePath: $"{Entry.ResPath}/scenes/ancients/puyuan_bg.tscn",
        VfxScenePath: string.Empty);

    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile => new(
        MapIconPath: $"{Entry.ResPath}/images/ancients/ancient_node_puyuan.png",
        MapIconOutlinePath: $"{Entry.ResPath}/images/ancients/ancient_node_puyuan_outline.png",
        RunHistoryIconPath: $"{Entry.ResPath}/images/ancients/puyuan_run_history.png",
        RunHistoryIconOutlinePath: $"{Entry.ResPath}/images/ancients/puyuan_run_history_outline.png",
        StageProcedural: null);

    // 只在第二章（Hive，Act Index = 1）出现。
    // 50% 出现概率由 HiveAncientPatch 负责（命中就只留蒲元，未命中只留原版三个），
    // 这里保持恒定 true —— 若在此再叠一次掷点，会变成 0.5×0.5 = 25%。
    public override bool IsValidForAct(ActModel act) => act is Hive;

    // 原版先古之民是“3 选 1”。RitsuLib 的 AncientEventInitialOptionsRegistryPatch 会
    // patch AncientEventModel.GenerateInitialOptionsWrapper，并直接用本方法返回的列表
    // 覆盖最终选项——所以这里返回几个，事件房就显示几个（之前返回全部 8 个，故显示 8 个）。
    // AllPossibleOptions 仍保留完整 8 件（供调试工具 / 校验使用），只在初始选项里随机取 3 件。
    private const int InitialRelicOptionCount = 3;

    public override IEnumerable<EventOption> AllPossibleOptions => BuildOptions();

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> pool = BuildOptions();

        if (pool.Count <= InitialRelicOptionCount)
            return pool;

        Rng.Chaotic.Shuffle(pool);
        return pool.Take(InitialRelicOptionCount).ToList();
    }

    private List<EventOption> BuildOptions() =>
    [
        CreateModRelicOption(ModelDb.Relic<TianLeiRen>().ToMutable(), "INITIAL"),
        CreateModRelicOption(ModelDb.Relic<ZhuQueYuShan>().ToMutable(), "INITIAL"),
        CreateModRelicOption(ModelDb.Relic<ShuiBoJian>().ToMutable(), "INITIAL"),
        CreateModRelicOption(ModelDb.Relic<HongDuanQiang>().ToMutable(), "INITIAL"),
        CreateModRelicOption(ModelDb.Relic<HunDuWanBi>().ToMutable(), "INITIAL"),
        CreateModRelicOption(ModelDb.Relic<LieCuiDao>().ToMutable(), "INITIAL"),
        CreateModRelicOption(ModelDb.Relic<DuanZaoTai>().ToMutable(), "INITIAL"),
        CreateModRelicOption(ModelDb.Relic<RongLu>().ToMutable(), "INITIAL"),
    ];
}
