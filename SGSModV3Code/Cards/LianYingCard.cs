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

// 连营：能力，费3（升级后 2），品质蓝，目标自身。
// 数据表：当你失去最后 1 张牌后，摸 1 张牌（升级只降费，效果不变）。
// 实现：挂 LianYingDrawPower，层数即「手牌空了摸几张」（这里固定 1）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class LianYingCard : SGSModV3BaseCard
{
    private const string CardsKey = "Cards";

    public LianYingCard() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new DynamicVar(CardsKey, 1m)
    };

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int drawCount = base.DynamicVars[CardsKey].IntValue;
        PowerCmd.Apply<LianYingDrawPower>(choiceContext, Owner.Creature, drawCount, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 数据表：升级只降费（2 -> 1）
        base.EnergyCost.UpgradeBy(-1);
    }
}
