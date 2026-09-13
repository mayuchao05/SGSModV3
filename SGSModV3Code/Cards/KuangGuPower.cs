using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 狂骨：技能，费2（升级1），蓝色品质（Uncommon），消耗。
// 数据表：你下一张攻击牌会给你回复等同于伤害量的血量。
// 参考酒（下一张攻击牌触发）的机制，但效果改为“回血”而非“翻倍”。
// 实现：打出后挂一次性状态 KuangGuPower；
//       用 BeforeCardPlayed 在伤害结算前标记“下一张攻击牌”，AfterDamageGiven 时对拥有者回血并移除自身。
[RegisterPower]
public sealed class KuangGuPower : ModPowerTemplate
{
    private sealed class Data
    {
        // 是否已标记“下一张攻击牌”（打出攻击后触发回血并解除）。
        public bool Armed;
    }

    protected override object InitInternalData() => new Data();

    public override PowerType Type => PowerType.Buff;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override PowerStackType StackType => PowerStackType.Counter;

    // 关键修正：必须在“整张牌结算前”武装，攻击牌的伤害才会在 AfterDamageGiven 里被吸血吃掉。
    // 若用 AfterCardPlayed，攻击牌的伤害已结算完、Armed 才被置 true，于是“第一张不吸血、第二张才吸”。
    // BeforeCardPlayed 在伤害发生前触发，完美对齐酒（下一张攻击牌）的机制。
    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        // 仅限本玩家打出的攻击牌（狂骨自身是技能牌，不会触发）。
        if (cardPlay.Card.Owner == base.Owner.Player && cardPlay.Card.Type == CardType.Attack)
        {
            base.GetInternalData<Data>().Armed = true;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature dealer, DamageResult result, ValueProp props, Creature target, CardModel cardSource)
    {
        // 仅响应本玩家造成的伤害；且必须处于“已标记下一张攻击牌”状态。
        if (dealer != base.Owner)
        {
            return;
        }
        Data data = base.GetInternalData<Data>();
        if (!data.Armed)
        {
            return;
        }
        data.Armed = false;
        // 回复等同于本次伤害量的生命。
        await CreatureCmd.Heal(base.Owner, (decimal)result.TotalDamage);
        await PowerCmd.Remove(this);
    }
}
