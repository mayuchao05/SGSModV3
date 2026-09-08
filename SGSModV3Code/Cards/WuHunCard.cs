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

// 武魂（临时占位 Power 卡，目的：解除商店 Power 栏位死锁）。
// 出牌获得 2 点力量（升级后 3 点）。
// 说明：游戏本体的商店货架生成器会为每个卡类型（含 Power）生成货架，
// 角色卡池里若没有任何 CardType.Power 的卡，Power 栏位找不到候选就会抛异常导致黑屏。
// 这张卡就是为了让卡池里有至少一张 Power 卡。后续可改名为数据表中的正式能力牌、
// 或扩展成带回合钩子的真正持久能力。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class WuHunCard : SGSModV3BaseCard
{
    private int _strength = 2;

    public WuHunCard() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, false)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 直接施加游戏内置的力量 Power，无需自建 ModPowerTemplate。
        PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, _strength, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => _strength += 1;
}
