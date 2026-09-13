using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

/// <summary>
/// 融火：能力，费 2（升级 1），金品质。
/// 获得状态“融火”：你造成的火焰伤害翻倍。
/// </summary>
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class RongHuoCard : SGSModV3BaseCard
{
    public RongHuoCard() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        PowerCmd.Apply<RongHuoPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 费用 2 -> 1
        base.EnergyCost.UpgradeBy(-1);
    }
}
