using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 驭雳（序号47）：能力，费3（升级费2），金。
// 效果由 Power 标记，实际转换逻辑在 SGSModV3Damage 中统一处理：
// 玩家造成非自损伤害时，非雷电伤害统一转为雷电伤害；本身就是雷电伤害则伤害 +2。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class YuLiCard : SGSModV3BaseCard
{
    public YuLiCard() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        PowerCmd.Apply<YuLiPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => base.EnergyCost.UpgradeBy(-1); // 3 -> 2
}
