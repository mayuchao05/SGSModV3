using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using SGSModV3.Ancients;

namespace SGSModV3.Patches;

/// <summary>
/// 蒲元事件房的背景场景路径由 EventModel.BackgroundScenePath 自动推导为
/// "events/background_scenes/" + Id.Entry.ToLowerInvariant()，即
/// res://scenes/events/background_scenes/sgs_mod_v3_event_pu_yuan_ancient.tscn。
/// 该文件在 mod/原版中均不存在，NEventRoom.SetupLayout 调用 CreateBackgroundScene()
/// 时会抛 AssetLoadException，导致事件房 UI 冻结（无对话、无选项）。
///
/// 本补丁在 get_BackgroundScenePath 返回后，若当前事件是蒲元且自动推导路径不存在，
/// 则按 [tezcatara, pael, orobas] 顺序尝试复用原版 Hive 先古之民的背景场景。
/// 若未来给蒲元做了自定义背景场景，该补丁会因为 FileExists 为 true 而保留原结果。
/// </summary>
[HarmonyPatch(typeof(EventModel), "get_BackgroundScenePath")]
public static class PuYuanBackgroundPatch
{
    private static readonly string[] FallbackEntries = { "tezcatara", "pael", "orobas" };

    /// <summary>蒲元自己的背景场景（mod 内），存在时优先使用。</summary>
    private const string CustomBackgroundPath = "res://SGSModV3/scenes/ancients/puyuan_bg.tscn";

    static void Postfix(EventModel __instance, ref string __result)
    {
        // 双重判定：类型 + ModelId.Entry，防止包装/克隆实例导致漏判。
        if (__instance is not PuYuanAncient && __instance?.Id?.Entry != "SGS_MOD_V3_EVENT_PU_YUAN_ANCIENT")
            return;

        // 1) 优先使用蒲元自己的背景场景。
        if (Godot.FileAccess.FileExists(CustomBackgroundPath))
        {
            if (__result != CustomBackgroundPath)
                Entry.Logger.Info($"[PuYuanBackgroundPatch] 蒲元使用自定义背景：{CustomBackgroundPath}（原推导：{__result}）");
            __result = CustomBackgroundPath;
            return;
        }

        // 2) 未来自定义了背景且文件存在 → 保留原结果。
        if (!string.IsNullOrEmpty(__result) && Godot.FileAccess.FileExists(__result))
            return;

        // 3) 兜底：复用原版 Hive 先古之民的背景场景。
        foreach (var entry in FallbackEntries)
        {
            var path = SceneHelper.GetScenePath($"events/background_scenes/{entry}");
            if (Godot.FileAccess.FileExists(path))
            {
                __result = path;
                Entry.Logger.Warn($"[PuYuanBackgroundPatch] 自定义背景缺失，回退到原版 Hive 先古：{path}");
                return;
            }
        }

        Entry.Logger.Warn($"[PuYuanBackgroundPatch] 找不到可用的背景，保留自动推导路径：{__result}");
    }
}
