using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 英姿：能力，费1。
// 基础效果：每回合开始时额外摸 1 张牌（持续整场战斗）。
//   实现：YingZiDrawPower（MOD 自建状态）：层数 = 每回合额外抽牌数，不随时间衰减。
//         打出两张 = 2 层 = 每回合额外摸 2 张。
// 升级效果：获得「固有」词条 —— 战斗开始时必定出现在起手手牌中。
//   参照内置卡「机器学习 MachineLearning」（故障机器人）：OnUpgrade => AddKeyword(CardKeyword.Innate)。
//   注：「固有 / 消耗」属于卡牌特性，由 Keyword 驱动，不写进卡面描述文本。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class YingZiCard : SGSModV3BaseCard
{
    public YingZiCard() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 每回合开始额外摸 1 张（可叠加）
        PowerCmd.Apply<YingZiDrawPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 与内置卡「机器学习」同款写法：升级后获得「固有」。
        // AddKeyword 直接修改实例关键字集（同族的 RemoveKeyword 已在“无中生有”上验证有效）。
        base.AddKeyword(CardKeyword.Innate);
    }
}
