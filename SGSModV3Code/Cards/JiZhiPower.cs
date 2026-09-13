using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 集智状态：每当本玩家打出1张技能牌后，摸1张牌。
// 集智自身的打出（能力牌）不会触发，因为它不是技能牌。
[RegisterPower]
public sealed class JiZhiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner.Player)
        {
            return;
        }
        if (cardPlay.Card.Type != CardType.Skill)
        {
            return;
        }
        // 用 base.Amount（Counter 叠加后=集智张数）决定摸几张：两张集智→摸两张。
        await CardPileCmd.Draw(choiceContext, base.Amount, base.Owner.Player, false);
    }
}
