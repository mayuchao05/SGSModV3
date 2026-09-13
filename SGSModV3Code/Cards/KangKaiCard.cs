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

// 慷慨：能力，费2（升级后仍2），品质蓝。
// 数据表：你每次被攻击时获得3点护甲（升级后5点）。
// 实现：打出后给玩家挂一个常驻状态 KangKaiPower，
//       该状态在玩家每次受到攻击后获得护甲（数值取自状态的 Amount，由升级决定）。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class KangKaiCard : SGSModV3BaseCard
{
    public KangKaiCard() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal amount = base.IsUpgraded ? 5m : 3m;
        PowerCmd.Apply<KangKaiPower>(choiceContext, Owner.Creature, amount, Owner.Creature, this, false);
        return Task.CompletedTask;
    }

    // 卡面描述注入护甲点数（基础 3 / 升级 5）。
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        description.Add("KangKaiBlock", base.IsUpgraded ? "5" : "3");
    }
}
