using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SGSModV3.Characters;

namespace SGSModV3.Relics;

// 混毒弯匕：你在回合内每打出 1 张攻击牌，临时获得 1 点力量。
// 用游戏内置 TemporaryStrengthPower：它内部转成 StrengthPower，并在本侧回合结束时收回，
// 正好对应“临时”语义（不用自己写 Power）。
[RegisterRelic(typeof(SGSModV3RelicPool))]
public sealed class HunDuWanBi : ModRelicTemplate
{
    private const decimal TempStrengthPerAttack = 1m;

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel? card = cardPlay.Card;
        if (card == null || card.Owner?.Creature != Owner.Creature)
        {
            return;
        }

        if (card.Type != CardType.Attack)
        {
            return;
        }

        // ① 真正加力量：StrengthPower 已确认注册正常（借刀杀人 / 南蛮入侵都在用），且支持负值。
        //    注意不能改回 TemporaryStrengthPower —— 它没注册进 ModelDb，Apply 会抛 KeyNotFoundException，
        //    异常会一路冒泡把整张牌的打出流程打断（表现就是遗物"完全没效果"）。
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, Owner.Creature, TempStrengthPerAttack, Owner.Creature, null, false);

        // ② 记一笔账：HunDuWanBiPower 只记录"本回合临时加了多少力量"，
        //    由它在本侧回合结束时把这部分力量收回去（见 HunDuWanBiPower）。
        await PowerCmd.Apply<HunDuWanBiPower>(
            choiceContext, Owner.Creature, 1m, Owner.Creature, null, false);
    }
}
