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

// 狂骨：技能，费2（升级1），蓝色品质（Uncommon），消耗。
// 数据表：你下一张攻击牌会给你回复等同于伤害量的血量。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class KuangGuCard : SGSModV3BaseCard
{
    public KuangGuCard() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        CardKeyword.Exhaust,
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        PowerCmd.Apply<KuangGuPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1); // 2 -> 1
    }
}
