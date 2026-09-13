using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 火攻：攻击，费1，品质蓝，单体敌人。
// 数据表：造成 4 点火焰伤害，你每有 1 张手牌伤害加 2（升级后 6 点，每张 +3）。
// 参考实现：决斗（伤害随手上牌数成长的攻击牌）。做法一致：结算时现算
// 「手牌张数 × 每张加成」，把算好的总数交给 DealFireDamage 的 DamageVar ——
// 不改 DynamicVars.Damage.BaseValue，避免加成永久累加到卡面上。
// 用火焰伤害结算，自动联动融火（翻倍）/铁索连环（AOE）/驭雳。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class HuoGongCard : SGSModV3BaseCard
{
    private const string BonusKey = "Bonus";

    public HuoGongCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new DamageVar(4m, ValueProp.Move),
        new DynamicVar(BonusKey, 2m)
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 本卡打出时已离手，统计的是剩余手牌。
        int cardsInHand = Owner.PlayerCombatState?.Hand.Cards.Count ?? 0;

        decimal perCard = base.DynamicVars[BonusKey].BaseValue;
        decimal total = base.DynamicVars.Damage.BaseValue + cardsInHand * perCard;

        await SGSModV3Damage.DealFireDamage(choiceContext, cardPlay.Target!, new DamageVar(total, ValueProp.Move), Owner.Creature, Owner, this, cardPlay);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);      // 4 -> 6
        base.DynamicVars[BonusKey].UpgradeValueBy(1m);   // 2 -> 3
    }
}
