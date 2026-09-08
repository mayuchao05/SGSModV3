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

// 桃园结义：消耗，全体友方恢复 5 点血量。升级后 8 点。
// 简单结算卡：对玩家角色回血（单人 MOD 友方即自身；若有队友可遍历 GetTeammatesOf）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class TaoYuanJieYiCard : SGSModV3BaseCard
{
    public TaoYuanJieYiCard() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(5m)
    ];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars.Heal.UpgradeValueBy(3m);
}
