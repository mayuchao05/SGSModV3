using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 集智：能力，费3（升级后2），品质金。
// 数据表：你每使用1张技能牌后，摸1张牌。
// 实现：打出后挂常驻状态 JiZhiPower，监听本玩家打出的技能牌。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class JiZhiCard : SGSModV3BaseCard
{
    public JiZhiCard() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        PowerCmd.Apply<JiZhiPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1); // 3 -> 2
    }
}
