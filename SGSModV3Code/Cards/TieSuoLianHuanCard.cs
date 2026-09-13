using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

/// <summary>
/// 铁索连环：能力，费 2（升级 1），金品质。
/// 获得状态“铁索连环”：你的属性伤害会命中所有敌人。
/// </summary>
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class TieSuoLianHuanCard : SGSModV3BaseCard
{
    public TieSuoLianHuanCard() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        PowerCmd.Apply<TieSuoLianHuanPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 费用 2 -> 1
        base.EnergyCost.UpgradeBy(-1);
    }
}
