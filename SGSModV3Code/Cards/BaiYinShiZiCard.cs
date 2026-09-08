using MegaCrit.Sts2.Core.Commands;
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

// 白银狮子：消耗，获得 10 点护甲，下回合开始时回复 3 点血量。升级后 13 点护甲，回复 5 点。
// 简单结算卡：先获得护甲（本回合）。"下回合回血"为回合钩子，暂以基础护甲实现，钩子后续补。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class BaiYinShiZiCard : SGSModV3BaseCard
{
    public BaiYinShiZiCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(10m, ValueProp.Move)
    ];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}
