using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 儒贤：技能，费0，基础牌（Basic）。固有 / 保留 / 消耗。
// Basic 稀有度 = 与原版 Defect「双重释放」同机制：仅作为初始卡牌，
// 不会出现在卡牌奖励 / 商店 / 药水等获取渠道。
// 数据表：本回合内你每打出1张牌，摸1张牌（升级：打出时额外获得1点能量）。
// 实现：打出后挂一个 RuXianPower（持续本回合，回合结束移除）；
//       升级版在打出时立即获得1点能量。
[RegisterCard(typeof(SGSModV3CardPool))]
[RegisterCharacterStarterCard(typeof(SGSModV3Character), 1)]
public sealed class RuXianCard : SGSModV3BaseCard
{
    public RuXianCard() : base(0, CardType.Skill, CardRarity.Basic, TargetType.Self, true)
    {
    }

    // 固有（起手必持）/ 保留（回合末不弃）/ 消耗（打出后移出本场战斗）。
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        CardKeyword.Innate,
        CardKeyword.Retain,
        CardKeyword.Exhaust,
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<RuXianPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        // 升级版：打出时立即获得1点能量。
        if (base.IsUpgraded)
        {
            await PlayerCmd.GainEnergy(1m, Owner);
        }
    }

    // 卡面描述：升级后额外说明「打出时获得1点能量」。
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        description.Add("RuXianUp", IsUpgraded ? "升级：打出时额外获得1点能量。" : "");
    }
}
