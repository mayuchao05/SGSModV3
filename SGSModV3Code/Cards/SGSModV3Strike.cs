using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SGSModV3;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 杀：SGSModV3 的基础攻击牌，对应三国杀“杀”。
// 以 CardRarity.Basic 注册为基础牌，基础牌不会出现在卡牌奖励池中（只作为初始牌组），
// 因此“杀”不会像普通卡那样在战斗后/事件里作为奖励出现。
[RegisterCard(typeof(SGSModV3CardPool))]
[RegisterCharacterStarterCard(typeof(SGSModV3Character), 3)]
public sealed class SGSModV3Strike : ModCardTemplate
{
    public SGSModV3Strike() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move)
    ];

    // 出牌视觉钩子：触发挥剑动画（基础牌不走 SGSModV3BaseCard，需单独挂）。
    public override Task OnEnqueuePlayVfx(Creature? target)
    {
        CardAnimTrigger.Play(this);
        return base.OnEnqueuePlayVfx(target);
    }

    // 直接通过战斗命令结算伤害（框架不会对基础牌以外的目标自动结算）。
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SGSModV3Damage.DealPhysicalDamage(choiceContext, cardPlay.Target!, DynamicVars.Damage, Owner.Creature, Owner, this, cardPlay);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);

    // 卡面图复用三国杀“杀”原图。
    public override string CustomPortraitPath => $"{Entry.ResPath}/images/cards/ShaCard.png";
}
