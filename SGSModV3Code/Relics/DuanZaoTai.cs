using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SGSModV3.Characters;

namespace SGSModV3.Relics;

// 锻造台：拾起时，随机升级 4 张攻击牌。
// 实现参考原版 Whetstone：筛牌 -> StableShuffle(Niche) -> Take -> 逐张 CardCmd.Upgrade。
[RegisterRelic(typeof(SGSModV3RelicPool))]
public sealed class DuanZaoTai : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(4)];

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    public override Task AfterObtained()
    {
        List<CardModel> candidates = PileType.Deck.GetPile(Owner).Cards
            .Where(c => c.Type == CardType.Attack && c.IsUpgradable)
            .ToList();

        IEnumerable<CardModel> picked = candidates
            .StableShuffle(Owner.RunState.Rng.Niche)
            .Take(DynamicVars.Cards.IntValue);

        foreach (CardModel card in picked)
        {
            CardCmd.Upgrade(card, CardPreviewStyle.HorizontalLayout);
        }

        return Task.CompletedTask;
    }
}
