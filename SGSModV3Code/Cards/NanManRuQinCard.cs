using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using SGSModV3.Characters;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 南蛮入侵：技能，费2，所有敌人失去 5 点力量（升级后 8 点）。
// 直接对全体敌人施加负力量（= 失去力量），确保稳定生效。
// （之前走 PowerVar 自动施加通道未生效，这里改用手动 PowerCmd.Apply，与武魂同款可靠写法。）
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class NanManRuQinCard : SGSModV3BaseCard
{
    private int _amount = 5;

    public NanManRuQinCard() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies, false)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 对全体敌人施加负力量（失去力量）。StrengthPower 支持负值（AllowNegative）。
        foreach (var enemy in this.GetTargets(Owner.Creature))
        {
            PowerCmd.Apply<StrengthPower>(choiceContext, enemy, -_amount, Owner.Creature, this, false);
        }
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => _amount += 3;
}
