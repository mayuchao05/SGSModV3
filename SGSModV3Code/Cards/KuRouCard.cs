using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 苦肉：能力，费 2，品质金。当你回合内失去生命时，摸 1 张牌并获得 1 点能量；升级后摸 2 张。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class KuRouCard : SGSModV3BaseCard
{
    public KuRouCard() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new CardsVar(1)
    };

    // 用 async + await 保证 KuRouPower 一定挂上（不 await 时若内部抛异常会被静默吞掉，
    // 表现为“打出后毫无效果”）。
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature ownerCreature = Owner.Creature;
        await PowerCmd.Apply<KuRouPower>(choiceContext, ownerCreature, DynamicVars.Cards.BaseValue, ownerCreature, this, false);
        Entry.Logger.Info($"[KuRou] 已挂上 KuRouPower，层数={DynamicVars.Cards.BaseValue}");
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
