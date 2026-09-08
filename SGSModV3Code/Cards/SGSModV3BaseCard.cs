using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SGSModV3.Characters;

namespace SGSModV3.Cards;

public abstract class SGSModV3BaseCard : ModCardTemplate
{
    protected SGSModV3BaseCard(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary = true)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 卡面图：走 CustomPortraitPath 字符串属性（0.111.0 推荐的“可选资源覆写”方式），
    // 框架会按 res://images/cards/{类名}.png 从本 MOD 的 pck 里取图。
    // 不再使用 CardAssetProfile 构造器 —— 0.111.0 已删除旧的重载，旧写法会在运行时抛 MissingMethodException，
    // 导致每张卡取能量图标时崩溃（表现为满屏卡牌显示异常）。
#pragma warning disable RITSU013 // 抽象基类无对应卡面图，具体卡由 GetType().Name 解析到各自的 png
    public override string CustomPortraitPath => $"{Entry.ResPath}/images/cards/{GetType().Name}.png";
#pragma warning restore RITSU013

}
