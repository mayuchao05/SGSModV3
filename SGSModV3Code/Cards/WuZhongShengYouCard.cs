using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 无中生有：抽 2 张牌，消耗。升级后不再消耗（抽牌数保持 2 张）。
// 升级移除“消耗”词条：RemoveKeyword 直接改实例关键字集（含标签与“打出后消耗”行为），
// 描述用 IfUpgradedVar 在升级后隐藏“消耗”文本。
[RegisterCard(typeof(SGSModV3CardPool))]
[RegisterCharacterStarterCard(typeof(SGSModV3Character), 1)]
public sealed class WuZhongShengYouCard : SGSModV3BaseCard
{
    public WuZhongShengYouCard() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 基础携带“消耗”关键字（构造函数 exhaust:true 也会加，这里显式声明以保证标签与行为一致）。
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get { yield return CardKeyword.Exhaust; }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2)
    ];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 通过牌堆命令正确抽牌（会触发抽牌相关事件/动画）。
        CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 升级：移除“消耗”词条（抽牌数保持 2 张不变）。
        this.RemoveKeyword(CardKeyword.Exhaust);
    }
}
