using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 绽火：攻击，费1，品质蓝。
// 数据表：消耗。对1名敌人造成6点火焰伤害，所有敌人获得2层虚弱与2层易伤。
// 升级：9点火焰伤害，所有敌人获得3层虚弱与3层易伤。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class ZhanHuoCard : SGSModV3BaseCard
{
    public ZhanHuoCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword> { CardKeyword.Exhaust };

    private const string DebuffKey = "ZhanHuoDebuff";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
        new DynamicVar(DebuffKey, 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature target = cardPlay.Target!;
        await SGSModV3Damage.DealFireDamage(choiceContext, target, DynamicVars.Damage, Owner.Creature, Owner, this, cardPlay);

        int debuffStacks = DynamicVars[DebuffKey].IntValue;
        foreach (Creature enemy in SGSModV3Damage.GetAliveEnemies(Owner.Creature))
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, debuffStacks, Owner.Creature, this, false);
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, debuffStacks, Owner.Creature, this, false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars[DebuffKey].UpgradeValueBy(1m);
    }
}
