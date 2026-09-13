using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 慷慨状态：每当拥有者被攻击（伤害结算前）获得护甲，护甲可抵挡该次伤害。
// 护甲量取自本状态的 Amount（由慷慨卡升级决定 3 / 5）。
[RegisterPower]
public sealed class KangKaiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile($"{Entry.ResPath}/images/powers/{GetType().Name}.png", $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override PowerStackType StackType => PowerStackType.Counter;

    // 用 BeforeDamageReceived：在伤害结算前获得护甲，护甲才能实际抵挡这次伤害。
    public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (target != base.Owner)
        {
            return;
        }
        await CreatureCmd.GainBlock(target, base.Amount, ValueProp.Move, null, false);
    }
}
