using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

/// <summary>
/// 闪电 Power：回合开始时你失去 1 点生命，一名随机敌人受到 {Amount} 点雷电伤害。
/// Amount = 卡面 DamageVar（9 / 升级 12）。扣血量固定 1，不随升级变化。
/// 回合钩子照抄龙怒（AfterPlayerTurnStart），随机目标照抄天劫（GetRandomEnemy + DealThunderDamage）。
/// </summary>
[RegisterPower]
public sealed class ShanDianPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != base.Owner.Player)
        {
            return;
        }

        // 失去 1 点生命（无视护甲，固定值）。
        await CreatureCmd.Damage(choiceContext, base.Owner, 1m, ValueProp.Unblockable, base.Owner);

        // 一名随机敌人受到 Amount 点雷电伤害。
        var target = SGSModV3Damage.GetRandomEnemy(Owner);
        if (target == null)
        {
            return;
        }
        await SGSModV3Damage.DealThunderDamage(choiceContext, target, new DamageVar(base.Amount, ValueProp.Move), Owner, Owner.Player);
    }
}
