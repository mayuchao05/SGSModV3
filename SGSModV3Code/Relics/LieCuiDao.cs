using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using SGSModV3.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SGSModV3.Characters;

namespace SGSModV3.Relics;

// 烈淬刀：每回合第 1 次对敌人造成伤害时，该伤害翻倍。
// 遗物本身不能挂 ModifyDamageMultiplicative（那是 PowerModel 的钩子），
// 所以每场战斗首次本侧回合开始时给自己挂一个隐藏 Power，由该 Power 完成翻倍并在回合开始复位。
[RegisterRelic(typeof(SGSModV3RelicPool))]
public sealed class LieCuiDao : ModRelicTemplate
{
    private bool _appliedThisCombat;

    public override RelicRarity Rarity => RelicRarity.Ancient;

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
        await PowerCmd.Apply<LieCuiDaoDoubleDamagePower>(
            choiceContext, Owner.Creature, 1m, Owner.Creature, null, false);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _appliedThisCombat = false;
        return Task.CompletedTask;
    }
}
