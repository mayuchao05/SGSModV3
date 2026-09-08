# SGSModV3 —— 三国杀主题《杀戮尖塔 2》MOD

把《三国杀》的卡牌体系搬进《杀戮尖塔 2》（Slay the Spire 2）：18 张原创卡牌、自定义角色、中英双语本地化，基于 Godot 4.5.1 + C# 与第三方框架 STS2RitsuLib 从零实现。

> 这是一个**逆向工程驱动**的 Mod 项目 —— 项目全程没有官方文档，所有 API 行为通过反编译游戏本体与框架源码确认。

---

## 🎴 已实现卡牌

### 基础牌（不进卡牌奖励池）

| 卡牌 | 类型 | 效果 |
|------|------|------|
| **杀** | 基础攻击 | 6 → 9 伤害，起始 ×5 |
| **闪** | 基础防御 | 5 → 8 护甲，起始 ×4 |
| **桃** | 技能 | 回复 3 → 5 生命，消耗，起始 ×2 |

### 卡池可获取

| 卡牌 | 类型 | 效果 |
|------|------|------|
| **火杀** | 攻击 | 单体 9 → 12 伤害 |
| **雷杀** | 攻击 | 单体 4 → 6 伤害，触发两次 |
| **万箭齐发** | 攻击（AOE） | 全体敌人 10 → 15 伤害 |
| **南蛮入侵** | 技能（AOE） | 全体敌人失去 5 → 8 点力量 |
| **无中生有** | 技能 | 抽 2 → 3 张牌 |
| **顺手牵羊** | 技能 | 回 1 能量 + 抽 1 → 2 张牌 |
| **五谷丰登** | 技能 | 全体友方各抽 1 张，消耗 |
| **桃园结义** | 技能 | 全体友方回复 6 → 9 生命，消耗 |
| **仁王盾** | 技能 | 获得 8 → 12 护甲 |
| **白银狮子** | 技能 | 获得 10 → 13 护甲 |
| **藤甲** | 能力 | 获得 5 → 7 层覆甲，失去 1 敏捷 |
| **兵粮寸断** | 技能 | 敌人获 1 虚弱 + 1 易伤（升级降费） |
| **乐不思蜀** | 技能 | 延迟类减益 |
| **英姿** | 能力 | 每回合开始额外摸 1 张（升级：回合结束保留 1 张手牌） |
| **武魂** | 能力 | 获得 2 → 3 点力量 |

**自定义角色**：HP 75、勾玉主题能量球（玉绿阴阳勾玉，自绘分层贴图）

---

## 🔧 三个关键技术问题（本项目的核心价值）

### 1. 版本错位导致的批量崩溃

**现象**：MOD 加载后大量异常，`godot.log` 中刷出 **76 处 `MissingMethodException`**，全部指向 `SGSModV3BaseCard.get_AssetProfile()`。

**根因**：存在两套并行的 RitsuLib 版本号体系 ——
- 构建期：`csproj` 用 `Version="*"` 从 NuGet 拉取 `STS2.RitsuLib`（0.4.x）
- 运行期：实际加载的是**创意工坊**的 RitsuLib（包版本 0.5.18，按游戏版本分目录 `lib/0.111.0`）

游戏 v0.111.0 删除了 `CardAssetProfile` 的简单构造器（9/11 参），只保留 27 参重载。旧写法 `new CardAssetProfile(path, null×10)` 解析到已删除的重载，导致每张卡取能量图标时崩溃。

**修复**：

```xml
<!-- 改直接引用运行时实际加载的 DLL，去掉 NuGet PackageReference -->
<Reference Include="STS2-RitsuLib"
  HintPath="$(Sts2Dir)\..\..\workshop\content\2868840\3747602295\lib\0.111.0\STS2-RitsuLib.dll"
  Private="false" />
```

卡面图不再走 `CardAssetProfile` 构造器，改为重写 `CustomPortraitPath` 字符串属性指向 `res://images/cards/{类名}.png`。

### 2. 商店黑屏死锁（日志里没有异常）

**现象**：进入商店瞬间全屏黑，只能强退，日志中**查不到任何 Exception**（原生渲染层硬崩，日志来不及 flush）。

**根因**：反编译游戏本体 `MegaCrit.Sts2.Core.Factories.CardFactory.CreateForMerchant` 后发现 —— 该方法会为**每种卡类型（含 `Power`）生成货架栏位**。若角色卡池中一张 `CardType.Power` 的卡都没有，Power 栏位从候选池抽不到合法卡，抛 `InvalidOperationException: Can't generate valid rarity for merchant card type Power`，货架场景 `_Ready` 直接崩溃。

**修复**：向卡池补入一张 `CardType.Power` 卡（武魂）。

> 教训：报错信息里列出的那 6 张"问题卡"是被随机抽去填 Power 栏位的**非 Power 卡**，是结果不是原因。

### 3. 零官方文档下的 API 逆向

**结论（已实测校正）：框架不会自动结算任何伤害。**

`ModCardTemplate` 出牌管线**不会**把 `CanonicalVars` 里的 `PowerVar` / `DamageVar` 自动施加到目标 —— 内置卡（如 `Haze`）能用这套机制是因为它们是 `sts2` 原生 `CardModel`，而 MOD 卡走 RitsuLib 包装层，路径不同。

**正确范式**：所有效果必须显式调用命令

```csharp
// 单体伤害
CreatureCmd.Damage(ctx, new[] { cardPlay.Target }, DynamicVars.Damage, Owner.Creature, this, cardPlay);

// AOE 群体伤害（GetTargets 是扩展方法，需 using STS2RitsuLib.Combat.CardTargeting;）
foreach (var e in this.GetTargets(Owner.Creature))
    CreatureCmd.Damage(ctx, new[] { e }, DynamicVars.Damage, Owner.Creature, this, cardPlay);

// 施加状态
PowerCmd.Apply<StrengthPower>(ctx, target, amount, Owner.Creature, this, false);

// 加血 / 护甲 / 回能 / 抽牌
CreatureCmd.Heal(creature, amount, isHpLoss: false);
CreatureCmd.GainBlock(creature, new BlockVar(amount, ValueProp.Move), cardPlay);
PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
CardPileCmd.Draw(ctx, DynamicVars.Cards.BaseValue, Owner, false);
```

**方法**：用 dnSpy 导出游戏与框架共 **6128 个 `.cs` 源文件**作为可检索的权威参考；对反射无法覆盖的成员，直接扫描 `sts2.dll` 元数据字符串堆确认。

例：手牌访问链在所有公开 XML 与参考 MOD 中均无示例，最终通过扫描字符串堆确认 `PlayerCombatState.Hand.Cards`（`GetHandPile` / `HandPile` / `CardsInHand` **均不存在**，盲写会导致编译失败）。

---

## 📌 其他踩坑记录

| 问题 | 结论 |
|------|------|
| 卡池注册 | 游戏启动预加载所有已注册卡模型，每张卡必须属于某卡池，否则 `InvalidProgramException: Card ... is not in any card pool`（主菜单都进不去） |
| 起始卡与奖励池隔离 | `RegisterCharacterStarterCard` 与 `RegisterCard` 是两个独立槽；让起始卡不进奖励池的正解是用 `CardRarity.Basic` 基础牌，独立起始卡专用池是死路 |
| 升级增删关键字 | `OnUpgrade` 里调 `this.RemoveKeyword(...)` / `AddKeyword(...)` —— 直接改实例关键字缓存。靠私有字段或条件式 `CanonicalKeywords` 无效（升级时缓存不刷新） |
| 升级感知描述 | 用 `{IfUpgraded:show(升级时文本|未升级文本)}` 格式化器，避免把数字写死导致升级后描述不变 |
| `ModTemporaryPowerTemplate` 签名 | `InternallyAppliedPower` / `AfterSideTurnEnd` 是 **public**；`IsPositive` / `LastForExtraTurns` 是 **protected**；`OriginModel` 返回类型必须是精确的 `AbstractModel` |

---

## 🛠 构建与部署

```bash
# 完整构建（导出 PCK 并复制到游戏 mods 目录）
dotnet build SGSModV3.csproj

# 仅编译 C#
dotnet build SGSModV3.csproj /p:RunPckExport=false /p:CopyModOnBuild=false
```

产物（dll + json + pck）自动部署到 `Slay the Spire 2/mods/SGSModV3/`。

**注意**：启动游戏须走 Steam（`steam://rungameid/2868840`），直接拉 exe 会被 AppID 校验拦截。

### 依赖版本

- Godot 4.5.1 Mono（MegaDot）
- STS2RitsuLib 0.5.18（创意工坊，`lib/0.111.0`）
- 游戏版本 v0.111.0

> RitsuLib 更新到新游戏版本时，`csproj` 中 `lib/<版本>` 路径需同步修改。

---

## 📁 目录结构

```
SGSModV3/
├── SGSModV3Code/
│   ├── Cards/          # 18 张卡牌实现
│   ├── Characters/     # 自定义角色与卡池
│   ├── Relics/         # 遗物
│   └── Entry.cs        # [ModInitializer] 入口
├── SGSModV3/
│   ├── images/cards/   # 卡面图
│   └── localization/   # zhs / eng 本地化
├── docs/               # RitsuLib 模板原始文档
└── SGSModV3.json       # Mod manifest
```

---

## 🚧 待实现

- 火杀 / 雷杀的火焰 / 雷电伤害类型（当前用物理数值占位）
- 白银狮子「下回合回血」（需回合钩子）
- 数据表剩余卡牌：过河拆桥、火攻、决斗、业炎、铁索连环、诸葛连弩

---

## 🙏 致谢

- [STS2-RitsuLib](https://github.com/BAKAOLC/STS2-RitsuLib) —— Mod 框架
- [Slay the Spire 2 Modding Tutorials](https://github.com/GlitchedReme/SlayTheSpire2ModdingTutorials)

本 Mod 为粉丝向非商业作品，《三国杀》与《杀戮尖塔 2》相关商标归各自权利方所有。
