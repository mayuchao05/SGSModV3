using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 乐不思蜀：技能，费3，消耗。眩晕一名敌人（升级后眩晕所有敌人）。
// 机制参考游戏内置的"吹哨"类眩晕卡，使用原生 CreatureCmd.Stun。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class LeBuSiShuCard : SGSModV3BaseCard
{
    public LeBuSiShuCard() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>();

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (IsUpgraded)
        {
            // 升级后：眩晕所有敌人
            IEnumerable<Creature> enemies = Owner.Creature.CombatState.GetOpponentsOf(Owner.Creature);
            foreach (Creature enemy in enemies.ToList())
            {
                CreatureCmd.Stun(enemy, null);
            }
        }
        else
        {
            // 单体眩晕选中的敌人
            CreatureCmd.Stun(cardPlay.Target, null);
        }
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() { }
}
