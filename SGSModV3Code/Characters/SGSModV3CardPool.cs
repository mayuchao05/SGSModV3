using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace SGSModV3.Characters;

public sealed class SGSModV3CardPool : TypeListCardPoolModel
{
    // 卡面边框材质：原版拼接规则是
    //   FrameMaterialPath = "res://materials/cards/frames/" + CardFrameMaterialPath + "_mat.tres"
    // 所以这里只能返回【裸名】（原版各池返回 card_frame_red/blue/green 等），
    // 不能返回完整 res:// 路径 —— 之前返回完整路径被拼成
    //   res://materials/cards/frames/res://SGSModV3/..._mat.tres
    // 加载失败，卡框回退成占位角色（铁甲战士）的红色。
    // 实际材质放在 mod PCK 根的 materials/cards/frames/SGSModV3_card_frame_mat.tres，
    // Godot 的 mod PCK 会与原版 res:// 虚拟目录合并，该路径可被原版拼接逻辑直接找到。
    // 染色参数在 .tres 内（翠绿 h=0.375, s=0.9, v=1.0，参考传世宝玉）。
    public override string? CardFrameMaterialPath => "SGSModV3_card_frame";

    // Title 和 EnergyColorName 是池子的稳定标识，不是玩家看到的角色名。
    // 自定义角色卡、遗物、药水池保持同一个 EnergyColorName，方便实验室和文本统一读取能量图标。
    public override string Title => "SGSModV3";
    public override string EnergyColorName => "SGSModV3";

    // 这里指定卡牌文本和大图使用的能量图标路径。
    // res://SGSModV3/... 里的 SGSModV3 是 PCK 资源目录，不是 C# namespace。
    public override string? BigEnergyIconPath => $"{Entry.ResPath}/images/characters/energy_big.png";
    public override string? TextEnergyIconPath => $"{Entry.ResPath}/images/characters/energy_text.png";

    public override Color DeckEntryCardColor => SGSModV3Character.ThemeColor;
    public override Color EnergyOutlineColor => new(0.05f, 0.25f, 0.12f);

    // false 表示这是角色专属卡池，不是事件/状态那类无色卡池。
    public override bool IsColorless => false;
}
