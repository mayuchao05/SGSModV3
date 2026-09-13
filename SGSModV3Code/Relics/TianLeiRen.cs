using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using SGSModV3.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SGSModV3.Characters;

namespace SGSModV3.Relics;

// 天雷刃：拾起时，将至多 5 张牌变化为雷杀。
// 实现参考原版 Claws（Ancient 稀有度）：牌组选牌 -> 预演用可变副本 -> CardCmd.Transform 统一替换。
[RegisterRelic(typeof(SGSModV3RelicPool))]
public sealed class TianLeiRen : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(5)];

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    public override async Task AfterObtained()
    {
        CardSelectorPrefs prefs = new(SelectionScreenPrompt, 0, DynamicVars.Cards.IntValue)
        {
            Cancelable = false,
            RequireManualConfirmation = true
        };

        IEnumerable<CardModel> selected = await CardSelectCmd.FromDeckForTransformation(
            Owner, prefs, c => new CardTransformation(c, CreateLeiSha(c, true)));

        List<CardTransformation> plan = selected
            .Select(c => new CardTransformation(c, CreateLeiSha(c, false)))
            .ToList();

        if (plan.Count == 0)
        {
            return;
        }

        await CardCmd.Transform(plan, Owner.PlayerRng.Transformations, CardPreviewStyle.HorizontalLayout);
    }

    private CardModel CreateLeiSha(CardModel original, bool forPreview)
    {
        CardModel card = forPreview
            ? ModelDb.Card<LeiShaCard>().ToMutable()
            : Owner.RunState.CreateCard<LeiShaCard>(Owner);

        if (original.IsUpgraded && card.IsUpgradable)
        {
            if (forPreview)
            {
                card.UpgradeInternal();
            }
            else
            {
                CardCmd.Upgrade(card, CardPreviewStyle.HorizontalLayout);
            }
        }

        return card;
    }
}
