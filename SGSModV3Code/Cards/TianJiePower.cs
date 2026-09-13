using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

/// <summary>
/// 天劫 Power：你每回合打出 5 张牌时，对随机敌人造成 4 点雷电伤害 5 次（升级 6 点 5 次）。
/// Amount=1 未升级，Amount=2 升级。
/// </summary>
[RegisterPower]
public sealed class TianJiePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    private decimal DamagePerHit => Amount >= 2 ? 6m : 4m;

    // 每回合出牌计数。
    private int _cardsPlayedThisTurn;

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side && participants.Contains(Owner))
        {
            _cardsPlayedThisTurn = 0;
        }
        return base.BeforeSideTurnStart(choiceContext, side, participants, combatState);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.AfterCardPlayed(choiceContext, cardPlay);

        if (cardPlay.Card.Owner != base.Owner.Player)
            return;

        _cardsPlayedThisTurn++;

        // 每打出 5 张牌触发一次。
        if (_cardsPlayedThisTurn % 5 != 0)
            return;

        var dmg = new DamageVar(DamagePerHit, ValueProp.Move);
        for (int i = 0; i < 5; i++)
        {
            var target = SGSModV3Damage.GetRandomEnemy(Owner);
            if (target == null)
                continue;
            await SGSModV3Damage.DealThunderDamage(choiceContext, target, dmg, Owner, Owner.Player);
        }
    }
}
