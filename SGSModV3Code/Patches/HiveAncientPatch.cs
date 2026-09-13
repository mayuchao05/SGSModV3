using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using SGSModV3.Ancients;

namespace SGSModV3.Patches;

/// <summary>
/// 第二层（Hive）的先古之民原本在 Hive.get_AllAncients() 里被硬编码为
/// [Orobas, Pael, Tezcatara]，且该方法没有调用基类（即没有走 RitsuLib 的
/// AllAncientsPostfix / AppendActAncients 注入）。因此 [RegisterActAncient]
/// 只能把蒲元加进“全局汇总列表”(ModelDb.AllAncients)，却进不了 Hive 自己的
/// 候选池——这就是事件房永远只显示 TEZCATARA 的根因。
///
/// 本补丁负责两件事：
///   1. 把蒲元放进 Hive 的候选池（RitsuLib 注入不到，只能 Harmony 补）。
///   2. 控制出现概率：命中 50% 时第二层就是蒲元，未命中则完全是原版三个，
///      两者互斥，不污染原版体验。
///
/// 为什么不“原版三个 + 追加蒲元”再由系统随机挑：
///   那样蒲元的实际出现率取决于候选池大小与游戏内部挑选算法（4 选 1 ≈ 25%），
///   不可控。这里直接在补丁里掷一次（每局只掷一次，见 PuYuanAppearanceRoll），
///   命中就只留蒲元、没命中就只留原版，结果是实打实的 50%。
///
/// 取蒲元的写法：用 ModelDb.AncientEvent&lt;PuYuanAncient&gt;()（内部走 ModelDb.Get&lt;T&gt;，
/// 与 ModelDb.Relic&lt;T&gt;() 同一套 mod 类型解析，已验证可用）。注意不能改成遍历
/// ModelDb.AllAncients 来查找——ModelDb.AllAncients 会反过来调用每个 act 的
/// get_AllAncients（含 Hive 自身），在补丁里访问它会无限递归。
/// </summary>
[HarmonyPatch(typeof(Hive), "get_AllAncients")]
public static class HiveAncientPatch
{
    static void Postfix(ref IEnumerable<AncientEventModel> __result)
    {
        // 通过 mod 类型解析拿到蒲元模型（与 ModelDb.Relic<T>() 同一机制，已验证可用）。
        var puyuan = ModelDb.AncientEvent<PuYuanAncient>();

        if (puyuan == null)
        {
            Entry.Logger.Warn("[HiveAncientPatch] 找不到蒲元模型(ModelDb.AncientEvent<PuYuanAncient>() 返回 null)，保留原版先古之民。");
            return;
        }

        // 每局只掷一次：命中则第二层出蒲元，未命中则保持原版三个。
        bool appear = PuYuanAppearanceRoll.ShouldAppear();

        if (appear)
        {
            __result = new List<AncientEventModel> { puyuan };
            Entry.Logger.Info("[HiveAncientPatch] 本局第二层先古之民 = 蒲元（50% 命中）。");
        }
        else
        {
            // 未命中：确保原版列表里没有蒲元（防止它被别处注入后残留）。
            __result = __result.Where(a => a is not PuYuanAncient).ToList();
            Entry.Logger.Info("[HiveAncientPatch] 本局第二层先古之民 = 原版（50% 未命中）。");
        }
    }
}
