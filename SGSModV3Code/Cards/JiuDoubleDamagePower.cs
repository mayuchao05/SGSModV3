using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 酒：技能，费1（升级后 0），品质蓝。
// 数据表：你下一张打出的攻击牌伤害翻倍；翻倍结算后本状态移除（状态栏酒图标随之消失）。
//
// 关键修正（上一版酒「每张攻击都翻倍、buff 常驻、跨回合」的根因）：
//   本 MOD 的攻击牌（杀/火杀/雷杀/决斗/万箭齐发）都是直接调 CreatureCmd.Damage 结算的，
//   框架【不会】为它们创建 AttackCommand。而官方的 GigantificationPower 范式
//   （BeforeAttack 捕获 AttackCommand + AfterAttack 移除）的钩子是挂在 AttackCommand
//   生命周期上的——在我的攻击流上 BeforeAttack/AfterAttack 永远不触发，于是
//   commandToModify 恒为 null，ModifyDamageMultiplicative 落到默认 return 2m，
//   于是「每张攻击都翻倍、且永不移除」。
//
// 正确做法（不依赖 AttackCommand）：
//   只用 ModifyDamageMultiplicative，配合 cardPlay 令牌。
//   CreatureCmd.Damage 文档明确：cardSource 在「预览」和「打出」时都会被设置，
//   但 cardPlay【只在卡牌真实打出时才非 null】。因此：
//     - cardPlay == null  → 预览/非卡牌伤害：不翻倍、也不消耗 buff（防预览误消耗）。
//     - cardPlay != null 且 consumedBy 为空 → 喝酒后第一张真实攻击牌：翻倍，记录
//       consumedBy = 本次 cardPlay（用于识别「同一张攻击牌的多次伤害」）。
//     - 同一张攻击牌的后续伤害（多目标 / 二次触发）→ cardPlay 相同 → 继续翻倍。
//     - 之后的攻击牌（cardPlay 不同）→ 已消耗 → 不再翻倍。
//   移除时机：① 玩家再次打出攻击牌时（不同 cardPlay）；② 或本回合结束（AfterSideTurnEnd）
//   且已消耗过 → 移除自身，图标消失。若整回合都没打出攻击（consumedBy 仍空），则保留到
//   下一回合，符合「下一张攻击牌」语义（不会无故跨回合乱翻倍）。
[RegisterPower]
public sealed class JiuDoubleDamagePower : ModPowerTemplate
{
    private sealed class Data
    {
        // 已触发翻倍的那次卡牌打出。为 null 表示酒还没翻倍过任何攻击。
        public CardPlay? consumedBy;
    }

    protected override object InitInternalData() => new Data();

    public override PowerType Type => PowerType.Buff;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageMultiplicative(Creature target, decimal amount, ValueProp props, Creature dealer, CardModel cardSource, CardPlay cardPlay)
    {
        // 只作用于「本玩家打出的、真正的攻击牌伤害」。
        if (cardSource == null)
        {
            return 1m;
        }
        if (cardSource.Owner?.Creature != base.Owner)
        {
            return 1m;
        }
        if (!props.IsPoweredAttack())
        {
            return 1m;
        }
        // 预览或非卡牌来源的结算：cardPlay 为 null，不翻倍、也不消耗 buff。
        if (cardPlay == null)
        {
            return 1m;
        }

        Data data = base.GetInternalData<Data>();

        // 第一张真实攻击牌：翻倍，并标记本次打出已消耗 buff。
        if (data.consumedBy == null)
        {
            data.consumedBy = cardPlay;
            return 2m;
        }
        // 同一张攻击牌的后续伤害（多目标 / 二次触发）：继续翻倍。
        if (data.consumedBy == cardPlay)
        {
            return 2m;
        }
        // 之后的攻击牌：buff 已消耗（移除动作可能还在异步进行），不再翻倍。
        _ = RemoveSelfSafe();
        return 1m;
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // 只在「自己这一侧」回合结束时清理；若本回合已消耗过（打出过被翻倍的攻击），
        // 移除自身让图标消失。若整回合都没打出攻击，则保留到下一回合（下一张攻击牌语义）。
        if (side != base.Owner.Side)
        {
            return Task.CompletedTask;
        }
        Data data = base.GetInternalData<Data>();
        if (data.consumedBy != null)
        {
            return RemoveSelfSafe();
        }
        return Task.CompletedTask;
    }

    // 在钩子里 fire-and-forget 地移除自身，避免阻塞伤害结算；
    // 包 try/catch 吞掉异常，防止移除失败影响本次伤害计算。
    private async Task RemoveSelfSafe()
    {
        try
        {
            await PowerCmd.Remove(this);
        }
        catch
        {
        }
    }
}
