using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 无懈可击：能力，费2（升级后 1），品质金，目标自身。
// 数据表：获得 1 层缓冲（升级只降费，层数不变）。
// 参考实现：故障机器人「缓冲 Buffer」——它用的是 PowerVar<BufferPower>(1)，
//   但 MOD 层的 PowerVar 不会自动施加（血泪教训），所以这里手动 PowerCmd.Apply。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class WuXieKeJiCard : SGSModV3BaseCard
{
    private const string BufferKey = "BufferPower";

    public WuXieKeJiCard() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new DynamicVar(BufferKey, 1m)
    };

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int stacks = base.DynamicVars[BufferKey].IntValue;
        PowerCmd.Apply<BufferPower>(choiceContext, Owner.Creature, stacks, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 数据表：升级只降费（2 -> 1），缓冲层数不变
        base.EnergyCost.UpgradeBy(-1);
    }
}
