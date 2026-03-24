Unity 游戏开发场景下的 Git 提交信息规范，给出**每个字段的详细说明**（包括规则、取值范围、示例和实操注意事项），以便团队能精准、统一地编写提交信息。

以下是适配 Unity 开发的 Git 提交信息各字段的完整、细化说明：

---

## Unity Git 提交信息 字段详细说明

核心格式回顾：

```
<type>(<scope>): <subject>

<body>

<footer>
```

### 1. type（提交类型，必填）

#### 核心规则

- 小写英文单词，无空格/特殊字符
- 精准描述「变更的本质」，而非「变更的内容」
- 优先使用 Unity 场景特有的类型，通用类型仅用于非资源/非功能类变更

#### 取值分类及说明（附 Unity 场景示例）

| 类型         | 适用场景                                                               | 典型示例                                   |
| ------------ | ---------------------------------------------------------------------- | ------------------------------------------ |
| **feat**     | 新增游戏功能/逻辑（非资源类）                                          | feat(Combat): 新增暴击伤害计算逻辑         |
| **fix**      | 修复 bug/逻辑错误（包括资源引用、组件参数错误）                        | fix(Player): 修复跳跃后无法落地的 bug      |
| **asset**    | 美术/音频/配置等资源的新增/修改/删除（模型、贴图、音效、配置表等）     | asset(Assets/Sound): 替换背景音乐文件      |
| **scene**    | 场景文件的修改（物体层级、光照、导航网格、场景参数等）                 | scene(Assets/Scenes/Level2): 调整场景光照  |
| **prefab**   | 预制体的新增/修改/删除（组件参数、子物体、引用关系等）                 | prefab(Assets/Prefabs/Bullet): 新增碰撞体  |
| **shader**   | 着色器、材质球的新增/修改/调试                                         | shader(Assets/Materials): 修复透明材质穿模 |
| **anim**     | 动画剪辑、动画控制器、混合树、状态机的修改                             | anim(Player): 新增跑步转向动画             |
| **ui**       | UGUI/UGUI/UI Toolkit 相关的界面、控件、布局修改                        | ui(UI/Battle): 调整血条显示位置            |
| **refactor** | 代码/资源重构（无功能新增/修复，仅优化结构/性能）                      | refactor(AudioManager): 拆分音频播放逻辑   |
| **docs**     | 注释、文档、说明文件的修改（如 README、代码注释、资源说明）            | docs(PlayerController): 补充组件参数注释   |
| **style**    | 代码格式调整（无逻辑变更，如缩进、命名规范）/UI 样式微调（非布局变更） | style(EnemyAI): 统一代码缩进为 4 空格      |
| **test**     | 测试相关（新增测试用例、调整自动化测试脚本、手动测试记录）             | test(Combat): 新增暴击逻辑单元测试         |
| **chore**    | 工具/配置类变更（非业务逻辑，如.gitignore、编辑器设置）                | chore: 更新.gitignore 忽略 Unity 日志文件  |
| **build**    | 打包/构建相关（平台配置、打包参数、SDK 版本、签名配置）                | build(Android): 调整包名和版本号           |
| **perf**     | 性能优化（代码/资源/渲染性能提升，如合批、GC 优化、模型减面）          | perf(Scene/Level1): 优化场景 DrawCall      |

#### 注意事项

- 避免「类型混用」：例如「新增玩家模型」应使用 `asset` 而非 `feat`；「修复 UI 按钮样式」应使用 `ui` 而非 `fix`（仅当按钮功能错误时用 `fix`）。
- 不要自定义未约定的 type：如「modify」「update」等，统一用上述分类。

### 2. scope（影响范围，必填）

#### 核心规则

- 括号内填写，无空格（如 `feat(Player):`）
- 精准定位「变更的具体模块/资源路径/功能点」，让团队一眼知道改了哪里
- 优先使用「模块名」「资源路径」「系统名」，避免模糊的描述（如「all」「global」）

#### 取值分类及说明（附示例）

| 分类          | 适用场景                                                       | 示例                                           |
| ------------- | -------------------------------------------------------------- | ---------------------------------------------- |
| **模块/功能** | 游戏核心功能模块（玩家、敌人、战斗、UI 子模块等）              | Player、Enemy、Combat、UI/MainMenu、SaveSystem |
| **资源路径**  | 具体的资源文件夹/文件（适配 Unity 资源管理规范，简写核心路径） | Assets/Models/Player、Assets/Scenes/Level1     |
| **系统/组件** | Unity 内置系统/自定义组件                                      | InputSystem、AudioManager、Rigidbody、Canvas   |
| **平台**      | 多平台适配相关的变更                                           | Platform/iOS、Platform/Android、Platform/PC    |

#### 注意事项

- 路径简写原则：无需写完整绝对路径，仅保留「核心识别部分」，如 `Assets/Scenes/Level1` 而非 `Assets/Game/Scenes/Level1.unity`。
- 避免过宽的 scope：如「UI」不如「UI/Battle」精准，「Model」不如「Model/Player」精准。
- 多个小范围变更：若变更涉及 2-3 个关联小模块，用 `/` 分隔（如 `feat(Player/Combat):`）；若涉及无关联的多个模块，拆分为多个提交。

### 3. subject（简短描述，必填）

#### 核心规则

- 紧跟 `):` 后，首字母小写，结尾无句号
- 动词原形开头，精准描述「做了什么」，而非「为什么做」
- 长度控制在 50 字符以内（Git 日志默认显示长度）
- 语言：统一使用英文（团队约定中文除外）

#### 常用动词及示例

| 动词     | 适用场景          | 示例描述                                |
| -------- | ----------------- | --------------------------------------- |
| add      | 新增内容/资源     | add double jump animation clip          |
| update   | 更新已有内容/参数 | update movement speed parameter         |
| fix      | 修复错误          | fix ui button click not trigger         |
| refactor | 重构代码/资源     | refactor enemy ai decision logic        |
| remove   | 删除无用内容/资源 | remove unused collider component        |
| optimize | 优化性能/资源     | optimize model polycount to 8k tris     |
| adjust   | 微调参数/布局     | adjust canvas scaler for mobile screens |

#### 错误 vs 正确示例

| 错误示例                          | 正确示例                                  | 错误原因                   |
| --------------------------------- | ----------------------------------------- | -------------------------- |
| feat(Player): 玩家可以二段跳了    | feat(Player): add double jump ability     | 用中文、无动词、描述不简洁 |
| fix(UI): 修复按钮问题             | fix(UI/MainMenu): fix button click delay  | 描述模糊、无具体问题       |
| asset(Model): Update player model | asset(Assets/Models): update player model | 首字母大写（无需大写）     |

### 4. body（详细说明，可选）

#### 核心规则

- 与 subject 空一行分隔，每行开头可加 `-` 列点说明
- 详细解释「为什么改」「改了什么细节」「影响范围」，补充 subject 未说明的信息
- 贴合 Unity 特性：重点说明资源引用、.meta 文件、组件调整、平台兼容性等细节

#### 填写要点（Unity 场景）

1. 变更原因：如「因移动端触摸区域过小，调整按钮尺寸」「因模型面数过高导致卡顿，优化减面」；
2. 具体修改：
   - 代码类：「新增 JumpState 枚举」「修改 Rigidbody 质量从 10 改为 5」；
   - 资源类：「同步 PlayerModel.meta 文件，避免 GUID 冲突」「替换材质球的 Albedo 贴图路径」；
3. 测试情况：「已在 iOS 16 测试通过」「场景光照烘焙耗时从 5min 缩短至 2min」；
4. 注意事项：「预制体结构调整，子物体层级变更，引用该预制体的场景需同步检查」。

#### 示例

```
fix(UI/Battle): fix health bar not update in real time

- Modify HealthBar script to listen OnHealthChanged event
- Update Canvas render mode from Screen Space - Overlay to Camera
- Sync .meta file for HealthBar prefab to fix GUID mismatch
- Tested on PC/Android/iOS, all platforms work normally
```

### 5. footer（收尾信息，可选）

#### 核心规则

- 与 body 空一行分隔
- 用于关联需求/BUG 单号、说明破坏性变更、标注特殊信息

#### 常用场景及示例

| 场景              | 格式/示例                                                                         |
| ----------------- | --------------------------------------------------------------------------------- |
| 关联 Issue/BUG 单 | Closes <br>Fixes <br>Related to                                                   |
| 破坏性变更        | BREAKING CHANGE: 预制体 Player 的 Animator 组件参数名修改，需同步更新所有引用场景 |
| 特殊说明          | NOTE: 本次修改涉及场景烘焙，需重新烘焙 Level1/Level2 场景                         |

#### 注意事项

- 破坏性变更（如预制体结构、核心函数参数修改）必须标注，避免团队集成时出问题；
- Issue 单号格式统一，便于 CI/CD 工具关联（如 Jira、GitLab Issues）。

---

### 总结

1. **type 精准化**：优先使用 Unity 特有类型（asset/scene/prefab/ui 等），避免通用类型滥用；
2. **scope 具体化**：定位到具体模块/资源路径，拒绝「模糊范围」（如 UI → UI/Battle）；
3. **描述简洁化**：subject 控制长度，body 补充 Unity 特有细节（.meta 文件、烘焙、平台测试等）；
4. **特殊变更标注**：破坏性变更、Issue 关联必须在 footer 中明确，降低团队协作风险。

遵循这套规范，能让 Unity 项目的 Git 提交记录清晰可追溯，尤其适合多人协作的游戏开发团队，便于后续代码/资源回溯、问题定位和版本管理。
