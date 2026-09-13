using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 化身：技能，费1，品质蓝，目标自身。
// 数据表：发现一张能力牌，本回合该能力牌消耗为 0；升级后给的是「已升级」的能力牌。
// 参考实现：故障机器人「白噪音 WhiteNoise」（随机把一张能力牌加入手牌，本回合消耗为 0）。
//   游戏里没有「发现」机制（Discover 只存在于炉石），所以按白噪音的做法：
//   从本角色卡池里随机挑一张能力牌 → 生成一份临时副本（已正式登记进战斗）→ 本张消耗设为 0 → 加入手牌。
//
// 关键修正（上一版化身打出后卡死的根因）：
//   旧写法 card.ToMutable() + 手动 generated.Owner = Owner + CardPileCmd.AddGeneratedCardToCombat(...)，
//   生成的卡虽然进了手牌堆，却【没有登记进 CombatState】。玩家手动打出时
//   AddDuringManualCardPlay 查 card.CombatState 为 null → 抛
//   "must be added to a CombatState before playing it"，随后回收费堆又抛
//   "must be added to a CombatState before adding it to this pile" → 战斗回合循环死锁、游戏卡死。
//   正确做法是用 ICombatState.CreateCard(canonical, owner)：它内部会
//     canonical.ToMutable() → CombatState.AddCard(card, owner)（被 RitsuLib 的
//     ModModelIdentityCombatStateAddCardPatch 拦截登记身份树）→ card.AfterCreated()，
//   从而把卡正式登记进当前战斗的 CombatState。之后再 CardPileCmd.Add 进手牌即可正常打出。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class HuaShenCard : SGSModV3BaseCard
{
    public HuaShenCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 「消耗」：打出后移出本场战斗（不再回手牌），否则凭空塞一张 0 费能力牌太超模。
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        CardKeyword.Exhaust
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> candidates = null;
        try
        {
            candidates = ModelDb.AllCards
                .Where(IsValidCandidate)
                .ToList();
        }
        catch (Exception ex)
        {
            Entry.Logger.Error($"[HuaShen] 挑选能力牌失败：{ex}");
        }

        if (candidates == null || candidates.Count == 0)
        {
            return;
        }

        // 注意：这里必须用 Rng.Chaotic（真随机），不能用 new Rng(Owner, base.Id)。
        // 后者是「按固定种子播种」的确定性随机：同一张化身每次打出的随机序列完全一样，
        // 于是每次都抽到同一张能力牌。Chaotic 才是每次都不同的战斗随机。
        CardModel picked = candidates[Rng.Chaotic.NextInt(candidates.Count)];

        // 走「战斗内造卡」正式路径：把候选能力牌（canonical）登记进当前 CombatState，
        // 生成一份归本玩家所有、可正常打出/结算的临时副本。
        // Owner.Creature.CombatState 是 ICombatState，其 CreateCard 会完成
        // ToMutable + 注册 + 设 Owner + AfterCreated 全部流程。
        CardModel generated = Owner.Creature.CombatState.CreateCard(picked, Owner);

        // 升级版化身：给的是升级后的能力牌。
        if (base.IsUpgraded && generated.IsUpgradable)
        {
            generated.UpgradeInternal();
            generated.FinalizeUpgradeInternal();
        }

        // 本回合（打出前）这张牌消耗为 0。
        generated.EnergyCost.SetUntilPlayed(0);

        // 加入手牌（顶张）。此时 generated.CombatState 已非 null，可被玩家正常打出。
        await CardPileCmd.Add(generated, PileType.Hand, CardPilePosition.Top);
    }

    private static bool IsValidCandidate(CardModel card)
    {
        if (card.Type != CardType.Power)
        {
            return false;
        }
        if (card.Rarity == CardRarity.Basic)
        {
            return false;
        }
        if (card.GetType() == typeof(HuaShenCard))
        {
            return false;
        }
        // 只从本 MOD 的卡里取（避免塞进其它角色的能力牌）。
        // 不能用 card.Pool is SGSModV3CardPool 判断——运行时 Pool 是 RitsuLib
        // 生成的另一份实例，类型对不上会恒为 false，导致候选永远为空、化身毫无效果。
        // 用命名空间过滤最稳：本 MOD 所有卡都在 SGSModV3.Cards 下。
        return card.GetType().Namespace == "SGSModV3.Cards";
    }

    protected override void OnUpgrade()
    {
        // 数据表：升级后给的是「升级后的能力牌」，费用与品质不变。
        // 具体逻辑见 OnPlay（base.IsUpgraded 分支）。
    }

    // 卡面描述需要区分基础和升级：基础给「能力牌」，升级给「升级后的能力牌」。
    // 这里直接注入字符串变量，本地化文本用 {HuaShenType} 占位。
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        description.Add("HuaShenType", IsUpgraded ? "升级后的能力牌" : "能力牌");
    }
}
