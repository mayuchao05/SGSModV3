# SGSModV3 开发日志与踩坑记录

> 项目：三国杀主题《杀戮尖塔 2》MOD  
> 路径：`C:\Users\DELL\Desktop\STS2Mods\SGSModV3`  
> 当前版本：0.9.0（52/56 张卡牌已实现）  
> 作者：苦苦林

---

## 一、项目概述

本项目是一个基于 `STS2-RitsuLib` 的《杀戮尖塔 2》（Slay the Spire 2）MOD，将《三国杀》主题卡牌、角色与事件引入塔中。当前已实现：

- 新增角色与主题卡池（52 张卡牌，含杀/闪/桃、万箭齐发、南蛮入侵、决斗、火攻、业炎、闪电等）
- 9 件遗物
- 火焰 / 雷电属性伤害体系
- 第二层原创先古之民事件「蒲元」
- 中英双语本地化
- 翠绿主题卡框

---

## 二、主要开发节点

### 1. 角色与初始卡组
- 角色 `SGSModV3Character`（HP 75、金币 99）
- 起始卡使用 `[RegisterCharacterStarterCard]` 特性注册
- 曾试图 override `ModCharacterTemplate.LocalStartingDeck`，发现该方法为 `sealed`（CS0239），改用特性方案

### 2. 卡牌系统（52 张）
- 基础牌：杀（Strike）、闪（Defend）、桃
- AOE：万箭齐发、南蛮入侵
- 锦囊：顺手牵羊、无中生有、决斗
- 属性伤害：火杀、火攻、业炎、雷杀、天劫、闪电
- 能力/状态牌：仁王盾、八卦阵、酒、英姿、龙怒、苦肉、化身、强袭、诈降、琴音等
- 未实现（按用户要求不再继续）：绝情、渐营、智哲、殁亡

### 3. 伤害与战斗系统
- 自定义伤害统一走 `SGSModV3Damage`
- 火焰伤害、雷电伤害分别对应 `DealFireDamage`、`DealThunderDamage`
- 为正确触发敌人「受击反应」（如花园曼受击加甲），在伤害前后手动调用 `Hook.BeforeAttack` / `Hook.AfterAttack`
- 失去生命类效果使用 `ValueProp.Unblockable`

### 4. 遗物
- 共 9 件，包含「传世宝玉」等原创设计
- 遗物 Power 统一继承 `ModPowerTemplate`

### 5. 蒲元事件
- 自定义先古之民（Ancient）事件「蒲元」
- 自定义背景场景 `scenes/ancients/puyuan_bg.tscn`
- 50% 概率出现在第二章（Hive），不覆盖原版事件；通过 `HiveAncientPatch` 实现

### 6. 本地化
- 中英双语文本已对齐
- cards / relics / powers / ancients 键数完全一致

---

## 三、踩过的坑与解决方案

### 1. 卡面描述显示为键名（Localization 失效）
**现象**：游戏中卡牌/角色描述显示为 `SGS_MOD_V3_CARD_...` 而非中文。  
**根因**：localization JSON 路径/命名不规范，或语言判断用 `CultureInfo.CurrentUICulture`。  
**解决**：
- 将 JSON 放到 `localization/zhs/`、`localization/eng/`
- 键名格式：`SGS_MOD_V3_CARD_{类名大写}.description`
- 语言判断改用 `LocManager.Instance.Language.StartsWith("zh")`

### 2. 万箭齐发 AOE 无效果
**现象**：AOE 卡牌打出后没有任何效果。  
**根因**：`CreatureCmd.Damage` 签名不完整，未传入 `card` 与 `cardPlay`，导致系统不识别为攻击/无法结算。  
**解决**：调用 `CreatureCmd.Damage(ctx, targets, damage, dealer, this, cardPlay)`，完整传入六个参数。

### 3. 敌人受击反应不触发（花园曼）
**现象**：用攻击牌打花园曼，它受击后不加护甲。  
**根因**：`CreatureCmd.Damage` 不走原生 `AttackCommand` 流程，不会触发 `Hook.BeforeAttack`/`AfterAttack`。  
**解决**：自定义 `AttackCommand` 实例，用 `Hook.BeforeAttack(combatState, cmd)` 与 `Hook.AfterAttack(...)` 包住伤害，并加 try/catch 防止钩子异常打断伤害。

### 4. 起始卡组无法 override
**现象**：`SGSModV3Character.LocalStartingDeck` 编译报错 CS0239（sealed）。  
**解决**：改用 `[RegisterCharacterStarterCard(typeof(SGSModV3Character), 数量)]` 特性。

### 5. 卡框材质加载失败（卡框变占位红色）
**现象**：卡框显示为铁甲战士红色。  
**根因**：`CardFrameMaterialPath` 返回了完整 `res://...` 路径，原版会在内部拼接，导致畸形路径。  
**解决**：返回裸名 `SGSModV3_card_frame`，材质文件放 `materials/cards/frames/SGSModV3_card_frame_mat.tres`。

### 6. 临时力量 Power 报 KeyNotFoundException
**现象**：遗物挂 `Apply<TemporaryStrengthPower>` 后，攻击牌效果完全丢失。  
**根因**：dll 里有类不等于注册进 ModelDb；`TemporaryStrengthPower` 未注册。  
**解决**：自己写计数 Power，调用 `Apply<StrengthPower>(+n)`，回合结束再 `Apply<StrengthPower>(-n)` 收回。

### 7. `mod.json` 依赖报错 old-style
**现象**：启动日志报 `Detected old-style dependencies without min version specified!`。  
**根因**：依赖 JSON key 错误写成 `version` + `minVersion`；游戏反序列化只认 snake_case 的 `min_version`。  
**解决**：改为 `{ "id": "STS2-RitsuLib", "min_version": "0.5.20" }`。

### 8. 蒲元出现概率变成 25%
**现象**：预期 50%，实际感觉极低。  
**根因**：同时在 `HiveAncientPatch` 和 `PuYuanAncient.IsValidForAct` 里掷点，变成 0.5 × 0.5。  
**解决**：只在 `HiveAncientPatch` 中掷一次，`IsValidForAct` 恒为 `act is Hive`。

### 9. 闪电卡 `using` 缺失
**现象**：新建 `ShanDianCard` / `ShanDianPower` 编译报 CS0103/CS0246。  
**解决**：补齐 `MegaCrit.Sts2.Core.Commands`、`MegaCrit.Sts2.Core.Localization.DynamicVars` 等 using。

### 10. 自定义 Power 图标空白
**现象**：自定义 Power 在游戏里图标空白。  
**根因**：原生 `PowerModel` 图标路径在 MOD 下为空。  
**解决**：继承 `ModPowerTemplate`，override `AssetProfile` 指向 `res://{ModId}/images/powers/{GetType().Name}.png`。

### 11. 卡面贴图规格
**现象**：卡图显示黑边或主体被裁。  
**解决**：统一输出 1000×760。竖图宽缩到 1000 后顶对齐裁 760；横图居中裁。

### 12. 测试脚手架清理
**现象**：测试卡、立牧反复出现在初始卡组。  
**解决**：删除 `TestDeckBootstrap.cs`、移除 `Entry.Initialize` 中的调用、移除 `SGSModV3Character.LocalStartingDeck` override，改为用 `[RegisterCharacterStarterCard]` 特性临时加卡，测完即删特性。

---

## 四、仍待后续决定的事项

1. **版本号**：当前 0.9.0，正式发布时可升至 1.0.0。
2. **缺失卡牌**：绝情、渐营、智哲、殁亡 4 张暂不实做，按用户要求停止。
3. **创意工坊封面**：已保存 `docs/workshop_cover.jpg`（1917×1076）与 `workshop_cover_1280x720.jpg`。
4. **实测项**：卡框翠绿已确认正常；蒲元自定义背景+3选1遗物建议再跑一局确认。
5. **旧 MOD 冲突**：`mods\` 目录下建议移走 SGSModV2 / YunoMod，避免与 V3 撞车。

---

## 五、构建命令

```bash
# 只编译 C#（不导出 PCK）
dotnet build SGSModV3.csproj /p:RunPckExport=false

# 完整构建并部署到游戏 mods 目录
dotnet build SGSModV3.csproj
```

构建成功标志：
- 0 个 error
- 日志末尾出现 `  [ DONE ] savepack`
- `Slay the Spire 2\mods\SGSModV3\SGSModV3.pck` 时间戳刷新

---

*日志更新于 2026-09-14*
