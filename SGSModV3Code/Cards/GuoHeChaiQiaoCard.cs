using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using SGSModV3.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SGSModV3.Cards;

// 过河拆桥（重做）：技能，费1，品质白（Common），消耗。
// 数据表：敌人获得 50 层虚弱；升级后改为对【全体敌人】施加 50 层虚弱。
// 虚弱用游戏内置 WeakPower；全体敌人通过 SGSModV3Damage.GetAliveEnemies 取得。
[RegisterCard(typeof(SGSModV3CardPool))]
public sealed class GuoHeChaiQiaoCard : SGSModV3BaseCard
{
    private const decimal WeakAmount = 50m;

    public GuoHeChaiQiaoCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    // 消耗：描述是卡牌特性，单独以 Keyword 展示，不写进描述文本。
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword>
    {
        CardKeyword.Exhaust,
    };

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.IsUpgraded)
        {
            // 升级后：对全体敌人施加 50 层虚弱。
            foreach (Creature enemy in SGSModV3Damage.GetAliveEnemies(Owner.Creature))
            {
                PowerCmd.Apply<WeakPower>(choiceContext, enemy, WeakAmount, Owner.Creature, this, false);
            }
        }
        else
        {
            // 升级前：对单体敌人施加 50 层虚弱。
            PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, WeakAmount, Owner.Creature, this, false);
        }
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        // 升级不改数值，仅把作用目标由单体扩展为全体敌人（见 OnPlay 分支）。
    }

    // 卡面描述按升级状态显示当前作用范围：单体=敌人，全体=所有敌人。
    // 用字符串 DynamicVar 占位，避免把两种状态都写死在描述里。
    // ⚠️ 语言判断不能用 CultureInfo.CurrentUICulture —— 游戏运行时它不跟随游戏内语言设置
    // （中文界面下曾是 en-US，导致卡面出现 "an enemy获得 50 层虚弱" 的中英混杂）。
    // 正解：读游戏自己的 LocManager（Language 形如 zhs/eng；CultureInfo 是其派生文化）。
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        var loc = LocManager.Instance;
        string lang = loc?.Language ?? string.Empty;
        bool isZh = lang.StartsWith("zh", System.StringComparison.OrdinalIgnoreCase)
                    || lang.StartsWith("zhs", System.StringComparison.OrdinalIgnoreCase)
                    || (loc?.CultureInfo?.TwoLetterISOLanguageName == "zh");
        string scope = base.IsUpgraded
            ? (isZh ? "所有敌人" : "all enemies")
            : (isZh ? "敌人" : "an enemy");
        description.Add("Scope", scope);
    }
}
