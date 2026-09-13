using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace SGSModV3.Ancients;

// 蒲元在第二章（Hive）的出现概率控制。
//
// 为什么不用 Harmony 补丁剔除 __result：
//   RitsuLib 自己会动态给 Hive.GetUnlockedAncients 打 Postfix 追加先古，
//   我再打一个 Postfix 的话，两者执行顺序不确定，可能被它重新加回来。
//   而 RitsuLib 的 GetUnlockedAncientsPostfix 最后一步会调用
//   ModAncientActValidityFilter.FilterForAct -> IModAncientActValidity.IsValidForAct，
//   所以只要在 IsValidForAct 里返回随机结果，就一定在“合并之后”生效，且顺序有保证。
//
// 每局只掷一次：以 RunState 实例为缓存键，保证同一局内重复询问结果稳定。
internal static class PuYuanAppearanceRoll
{
    private const double AppearChance = 0.5;

    private static object? _cacheKey;
    private static bool _cachedResult;

    public static bool ShouldAppear()
    {
        RunState? state = null;
        try
        {
            state = RunManager.Instance?.DebugOnlyGetState();
        }
        catch
        {
            state = null;
        }

        // 不在局内（例如调试面板、图鉴预览）：随便给一个结果即可，不影响正式流程。
        if (state == null)
        {
            return Rng.Chaotic.NextFloat(1f) < AppearChance;
        }

        if (!ReferenceEquals(_cacheKey, state))
        {
            _cacheKey = state;
            _cachedResult = state.Rng.Niche.NextFloat(1f) < AppearChance;
        }

        return _cachedResult;
    }
}
