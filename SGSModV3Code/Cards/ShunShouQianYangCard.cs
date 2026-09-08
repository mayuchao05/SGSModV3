using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 顺手牵羊：获得 1 点能量并抽 1 张牌。升级后抽 2 张。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class ShunShouQianYangCard : SGSModV3BaseCard
{
    public ShunShouQianYangCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new CardsVar(1)
    ];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 通过玩家命令正确获得能量（读 DynamicVars，升级后仍为 1 点）。
        PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        // 通过牌堆命令正确抽牌（读 DynamicVars，升级后抽 2 张）。
        CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 升级后改为抽 2 张牌。
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
