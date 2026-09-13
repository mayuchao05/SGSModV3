using MegaCrit.Sts2.Core.CardSelection;
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
using System.Linq;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 制衡：技能，费0，品质蓝，目标自身。
// 数据表：弃置任意张牌并摸等量的牌；升级后若弃置了所有牌则额外摸一张。
// 参考实现：药水「赌徒特酿 GamblersBrew」（弃任意张牌，然后摸等量的牌）。
//   选牌用 CardSelectCmd.FromHand（min=0 表示可以一张都不选），
//   结算直接用现成的 CardCmd.DiscardAndDraw(ctx, 弃掉的牌, 等量张数)。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class ZhiHengCard : SGSModV3BaseCard
{
    public ZhiHengCard() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IReadOnlyList<CardModel> hand = Owner.PlayerCombatState?.Hand.Cards ?? new List<CardModel>();
        int handCount = hand.Count;
        if (handCount == 0)
        {
            return;
        }

        // 可以弃 0 ~ 全部张（min=0 才允许「一张都不选」）。
        CardSelectorPrefs prefs = new(CardSelectorPrefs.DiscardSelectionPrompt, 0, handCount);
        IEnumerable<CardModel>? chosen = await CardSelectCmd.FromHand(
            choiceContext, Owner, prefs, null, this);

        List<CardModel> toDiscard = chosen?.ToList() ?? new List<CardModel>();
        if (toDiscard.Count == 0)
        {
            return;
        }

        // 弃掉这些牌，摸等量的牌（DiscardAndDraw 内部会先弃后摸）。
        await CardCmd.DiscardAndDraw(choiceContext, toDiscard, toDiscard.Count);

        // 升级：若把手牌全弃光了，额外再摸一张。
        if (base.IsUpgraded && toDiscard.Count >= handCount)
        {
            await CardPileCmd.Draw(choiceContext, 1m, Owner, false);
        }
    }

    protected override void OnUpgrade()
    {
        // 数据表：升级后「若弃置了所有牌则额外摸一张」，费用/品质不变。
        // 具体逻辑见 OnPlay（base.IsUpgraded 分支）。
    }

    // 卡面描述只在升级后显示额外说明：基础版空，升级后显示括号里的升级效果。
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        description.Add("ZhiHengExtra", IsUpgraded ? "（升级：若弃光了所有手牌，则额外摸一张）" : "");
    }
}
