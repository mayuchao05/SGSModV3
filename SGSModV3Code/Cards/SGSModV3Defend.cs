using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 闪：SGSModV3 的基础防御牌，对应三国杀“闪”。
// 以 CardRarity.Basic 注册为基础牌，基础牌不会出现在卡牌奖励池中（只作为初始牌组）。
[RegisterCard(typeof(SGSModV3CardPool))]
[RegisterCharacterStarterCard(typeof(SGSModV3Character), 4)]
public sealed class SGSModV3Defend : ModCardTemplate
{
    public SGSModV3Defend() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self, false)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5m, ValueProp.Move)
    ];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);

    // 卡面图复用三国杀“闪”原图。
    public override string CustomPortraitPath => $"{Entry.ResPath}/images/cards/ShanCard.png";
}
