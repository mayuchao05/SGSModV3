using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SGSModV3.Characters;

namespace SGSModV3.Relics;

// 水波剑：战斗开始时，获得 4 层再生。
// 遗物没有“战斗开始带 ctx”的钩子（BeforeCombatStart 无 PlayerChoiceContext），
// 因此改用本侧回合开始钩子的首次触发来施放，并在战斗结束时复位标志。
[RegisterRelic(typeof(SGSModV3RelicPool))]
public sealed class ShuiBoJian : ModRelicTemplate
{
    private const decimal RegenStacks = 4m;

    private bool _appliedThisCombat;

    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<RegenPower>(RegenStacks)];

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (_appliedThisCombat || side != Owner.Creature.Side || !participants.Contains(Owner.Creature))
        {
            return;
        }

        _appliedThisCombat = true;
        Flash();
        await PowerCmd.Apply<RegenPower>(choiceContext, Owner.Creature, RegenStacks, Owner.Creature, null, false);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _appliedThisCombat = false;
        return Task.CompletedTask;
    }
}
