using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 连破：能力，费1（升级仍1），品质蓝（Uncommon）。
// 数据表：1名敌人死亡后，获得3点能量、摸4张牌（升级：4能量、6牌）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class LianPoCard : SGSModV3BaseCard
{
    public LianPoCard() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal energy = base.IsUpgraded ? 4m : 3m;
        decimal draw = base.IsUpgraded ? 6m : 4m;
        // 挂状态（已存在则叠加层数 Amount）。叠加时 AfterApplied 不会触发，
        // 所以本卡实际能量/抽牌必须在这里累加进真实实例的 DynamicVars。
        await PowerCmd.Apply<LianPoPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        LianPoPower power = Owner.Creature.GetPower<LianPoPower>();
        if (power != null)
        {
            power.DynamicVars["Energy"].BaseValue += energy;
            power.DynamicVars["Draw"].BaseValue += draw;
        }
    }

    // 卡面描述随升级变化（基础 3 能量 4 牌 / 升级 4 能量 6 牌）。
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        description.Add("LianPoEnergy", base.IsUpgraded ? 4m : 3m);
        description.Add("LianPoDraw", base.IsUpgraded ? 6m : 4m);
    }
}
