using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

/// <summary>
/// 寂灭 Power：回合结束时，若本回合造成了至少 16 点雷电伤害，对一名敌人造成 30 点雷电伤害（升级 45 点）。
/// 阈值/伤害数字通过卡牌升级 Amount 注入：Amount=1 表示未升级，Amount=2 表示升级。
/// 这样避免硬编码两张 Power。
/// </summary>
[RegisterPower]
public sealed class JiMiePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    // 升级阈值/伤害：未升级 Amount=1，升级 Amount=2。
    private decimal Threshold => Amount >= 2 ? 24m : 16m;
    private decimal DamageAmount => Amount >= 2 ? 45m : 30m;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            SGSModV3Damage.ResetThunderDamageThisTurn(Owner.Player);
        }
        return base.AfterPlayerTurnStart(choiceContext, player);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        await base.AfterSideTurnEnd(choiceContext, side, participants);

        if (side != Owner.Side)
            return;

        if (SGSModV3Damage.GetThunderDamageThisTurn(Owner.Player) < Threshold)
            return;

        var target = SGSModV3Damage.GetRandomEnemy(Owner);
        if (target == null)
            return;

        var dmg = new DamageVar(DamageAmount, ValueProp.Move);
        await SGSModV3Damage.DealThunderDamage(choiceContext, target, dmg, Owner, Owner.Player);
    }
}
