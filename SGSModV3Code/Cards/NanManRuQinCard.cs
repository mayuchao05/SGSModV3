using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using SGSModV3.Characters;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 南蛮入侵：技能，费2，品质蓝。
// 数据表：所有敌人失去 2 点力量（升级后 3 点）。
// 实现：数值放在 DynamicVar（键 "StrengthLoss"）里，出牌时读取并手动施加负力量。
// 说明：描述里的 {StrengthLoss:diff()} 令牌依赖 CanonicalVars 声明的 Var，
//       之前 CanonicalVars 返回空列表导致卡面数字渲染为空白，现按内置卡 DarkShackles 的范式修复。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class NanManRuQinCard : SGSModV3BaseCard
{
    private const string StrengthLossKey = "StrengthLoss";

    public NanManRuQinCard() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new DynamicVar(StrengthLossKey, 2m)
    };

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int loss = base.DynamicVars[StrengthLossKey].IntValue;
        // 对全体敌人施加负力量（= 失去力量）。StrengthPower 支持负值（AllowNegative）。
        foreach (var enemy in this.GetTargets(Owner.Creature))
        {
            PowerCmd.Apply<StrengthPower>(choiceContext, enemy, -loss, Owner.Creature, this, false);
        }
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars[StrengthLossKey].UpgradeValueBy(1m);
    }
}
