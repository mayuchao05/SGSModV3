using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

/// <summary>
/// 雷击 Power：持有者每格挡1次攻击，对1名随机敌人造成 Amount 点雷电伤害。
/// Amount 由 LeiJiCard 注入（3/5）。
/// </summary>
[RegisterPower]
public sealed class LeiJiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
    {
        await base.AfterDamageReceived(choiceContext, target, result, props, dealer, cardSource);

        // 只统计玩家格挡敌人的伤害。
        if (target != Owner)
            return;
        if (result.BlockedDamage <= 0)
            return;

        var enemy = SGSModV3Damage.GetRandomEnemy(Owner);
        if (enemy == null)
            return;

        var dmg = new DamageVar(Amount, ValueProp.Move);
        await SGSModV3Damage.DealThunderDamage(choiceContext, enemy, dmg, Owner, Owner.Player);
    }
}
