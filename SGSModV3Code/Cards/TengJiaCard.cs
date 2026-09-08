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

// 藤甲：能力，费1。
// 效果：获得 5 层覆甲（回合结束获得等量格挡），但失去 1 点敏捷（影响格挡成长）。升级后覆甲 5 -> 7。
// 覆甲用原生的 PlatingPower；敏捷用 DexterityPower（负值即减敏捷）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class TengJiaCard : SGSModV3BaseCard
{
    private int _plating = 5;

    public TengJiaCard() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, false)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 _plating 层覆甲
        PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature, _plating, Owner.Creature, this, false);
        // 失去 1 点敏捷（负敏捷）
        PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, -1m, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => _plating += 2;
}
