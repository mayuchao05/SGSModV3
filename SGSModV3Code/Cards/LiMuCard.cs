using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 立牧：技能，费1，品质蓝（Uncommon），消耗。
// 数据表：消耗，回复3（升级5）点血量，跳过你的下个回合，本回合你攻击牌的消耗为0。
//
// 实现要点：
//  - “跳过下个回合”不能用 CreatureCmd.Stun —— Stun 底层 StunInternal 在玩家（Monster==null）时
//    直接抛 “Can't stun a player”，且眩晕本质是替换怪物行动，对玩家无效。
//  - 改为挂一个一次性状态 LiMuSkipTurnPower：它在「下个回合开始」(AfterSideTurnStart) 时
//    让玩家立即结束回合（标准“失去回合”做法），随后自移除。
//  - “本回合攻击牌消耗为0”用 LiMuZeroCostPower（Late 钩子压下其它降费），回合结束移除。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class LiMuCard : SGSModV3BaseCard
{
    public LiMuCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    // 消耗（Exhaust）。
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        CardKeyword.Exhaust,
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal heal = base.IsUpgraded ? 5m : 3m;
        await CreatureCmd.Heal(Owner.Creature, heal);
        // 本回合攻击牌消耗为0（回合结束移除）。
        await PowerCmd.Apply<LiMuZeroCostPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        // 下个回合开始即结束回合（跳过下回合）。
        await PowerCmd.Apply<LiMuSkipTurnPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
    }

    // 卡面描述注入回复量（3 / 5）。
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        description.Add("LiMuHeal", base.IsUpgraded ? "5" : "3");
    }
}
