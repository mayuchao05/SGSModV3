using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 遗计：你每回合第 1 次受到伤害后，摸 2 张牌。
// 参考实现：遗物「百年积木 CentennialPuzzle」——
//   用 AfterDamageReceived 触发，用一个「本场/本回合是否已用过」的标记保证只触发一次。
//   区别在于百年积木是「每场战斗一次」，本卡是「每回合一次」，
//   所以复位放在 AfterSideTurnStart（自己这侧回合开始时清零）。
[RegisterPower]
public sealed class YiJiDrawPower : ModPowerTemplate
{
    private sealed class Data
    {
        public bool UsedThisTurn;
    }

    protected override object InitInternalData() => new Data();

    public override PowerType Type => PowerType.Buff;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (target != base.Owner)
        {
            return;
        }
        // 被护甲全挡（WasFullyBlocked / UnblockedDamage==0）或被缓冲抵消时，实际没掉血，
        // 不摸牌，也不消耗本回合的「第 1 次」次数。
        if (result == null || result.UnblockedDamage <= 0)
        {
            return;
        }
        Data data = base.GetInternalData<Data>();
        if (data.UsedThisTurn)
        {
            return;
        }
        data.UsedThisTurn = true;

        if (base.Owner.Player == null)
        {
            return;
        }
        await CardPileCmd.Draw(choiceContext, base.Amount, base.Owner.Player, false);
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != base.Owner.Side)
        {
            return Task.CompletedTask;
        }
        if (!participants.Contains(base.Owner))
        {
            return Task.CompletedTask;
        }
        base.GetInternalData<Data>().UsedThisTurn = false;
        return Task.CompletedTask;
    }
}
