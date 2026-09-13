using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SGSModV3.Characters;

namespace SGSModV3.Relics;

// 红缎枪：每 2 场普通战斗后，随机升级 1 张牌。
// 实现参考原版 FishingRod（Ancient）：只统计普通战斗，用 Niche 随机流抽牌，计数在遗物上显示。
[RegisterRelic(typeof(SGSModV3RelicPool))]
public sealed class HongDuanQiang : ModRelicTemplate
{
    private const int CombatsPerUpgrade = 2;

    private int _combatsSeen;

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool ShowCounter => true;

    public override int DisplayAmount => _combatsSeen % CombatsPerUpgrade;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (room.Encounter.RoomType != RoomType.Monster)
        {
            return Task.CompletedTask;
        }

        _combatsSeen++;
        InvokeDisplayAmountChanged();

        if (_combatsSeen % CombatsPerUpgrade != 0)
        {
            return Task.CompletedTask;
        }

        List<CardModel> candidates = PileType.Deck.GetPile(Owner).Cards
            .Where(c => c.IsUpgradable)
            .ToList();

        if (candidates.Count == 0)
        {
            return Task.CompletedTask;
        }

        CardModel card = Owner.RunState.Rng.Niche.NextItem(candidates);
        if (card == null)
        {
            return Task.CompletedTask;
        }

        Flash();
        CardCmd.Upgrade(card, CardPreviewStyle.HorizontalLayout);
        return Task.CompletedTask;
    }
}
