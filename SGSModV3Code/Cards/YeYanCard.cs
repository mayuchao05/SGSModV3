using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 业炎：攻击，费2，品质蓝，单体敌人，消耗（Keyword，不写进描述）。
// 数据表：消耗你的所有手牌，每消耗 1 张手牌就造成 6 点火焰伤害（升级 9 点）。
// 参考原版铁甲战士「恶魔之焰 Fiend Fire」：结算时统计当前手牌张数，逐张
// 消耗，最后把「张数 × 每张伤害」一次性以火焰伤害打出去（走 DealFireDamage，
// 自动联动融火/铁索连环/驭雳）。本卡自身在 OnPlay 时已离手，不计入张数。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class YeYanCard : SGSModV3BaseCard
{
    public YeYanCard() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword> { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move)   // 每张手牌造成的火焰伤害
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = Owner.PlayerCombatState?.Hand;
        if (hand == null || hand.Cards.Count == 0)
            return;

        // 先快照再逐张消耗，避免消耗过程中改动手牌集合导致遍历异常。
        List<CardModel> toExhaust = hand.Cards.ToList();
        decimal perCard = DynamicVars.Damage.BaseValue;
        decimal total = perCard * toExhaust.Count;

        foreach (CardModel card in toExhaust)
            await CardCmd.Exhaust(choiceContext, card, false, false);

        await SGSModV3Damage.DealFireDamage(choiceContext, cardPlay.Target!, new DamageVar(total, ValueProp.Move), Owner.Creature, Owner, this, cardPlay);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);   // 6 -> 9
}
