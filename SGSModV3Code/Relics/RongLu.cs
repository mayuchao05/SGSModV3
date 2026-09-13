using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SGSModV3.Characters;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace SGSModV3.Relics;

// 熔炉：拾起时，从牌组中移除 1 张牌，并获得 150 金币。
// 实现参考原版 PreciseScissors（移除）+ GoldenPearl（金币）。
[RegisterRelic(typeof(SGSModV3RelicPool))]
public sealed class RongLu : ModRelicTemplate
{
    private const decimal GoldReward = 150m;

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new GoldVar(150)];

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    public override async Task AfterObtained()
    {
        CardSelectorPrefs prefs = new(CardSelectorPrefs.RemoveSelectionPrompt, DynamicVars.Cards.IntValue);

        IEnumerable<CardModel> removed = await CardSelectCmd.FromDeckForRemoval(Owner, prefs, null);
        foreach (CardModel card in removed)
        {
            await CardPileCmd.RemoveFromDeck(card, true);
        }

        await PlayerCmd.GainGold(GoldReward, Owner, false);
    }
}
