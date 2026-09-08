using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 英姿：能力，费1。
// 基础效果：每回合开始时额外摸 1 张牌（持续整场战斗）。
//   实现：ClarityPower = “未来 N 个回合开始时各额外摸 1 张牌”，用足够大的 N 覆盖整场战斗。
// 升级效果：在基础上，每个回合结束时令一张手牌获得“保留”（下回合仍留手中）。
//   实现：额外施加一个 YingZiRetainPower（临时能力，挂在玩家身上，回合结束触发）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class YingZiCard : SGSModV3BaseCard
{
    // 覆盖整场战斗所需的回合数（典型战斗不会超过此值）
    private const int REST_OF_COMBAT_TURNS = 99;

    public YingZiCard() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, false)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 每回合开始额外摸 1 张
        PowerCmd.Apply<ClarityPower>(choiceContext, Owner.Creature, REST_OF_COMBAT_TURNS, Owner.Creature, this, false);

        // 升级：回合结束保留一张手牌
        if (IsUpgraded)
        {
            PowerCmd.Apply<YingZiRetainPower>(choiceContext, new[] { Owner.Creature }, 0, Owner.Creature, this, false);
        }

        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 升级仅改变效果（附加“回合结束保留一张牌”），不改变费用/数值。
    }
}
