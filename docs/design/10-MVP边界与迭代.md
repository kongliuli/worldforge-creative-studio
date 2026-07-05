# 10 — MVP 边界与迭代

> **立项参考**：[reference/09-MVP范围与路线图.md](./reference/09-MVP范围与路线图.md)、[reference/12-风险与刻意边界.md](./reference/12-风险与刻意边界.md)

---

## 一、产品边界（不变）

**WorldForge MVP** = Creative 租户 + 本地矛盾检测 + 世界构建与写作一体 — 详见 reference/09。

**刻意不做**（月 0–4）：Yjs、MAUI、整章 AI 代写、五场景 GTM — 详见 reference/12。

---

## 二、代码实现状态（2026-07-05 骨架）

### ✅ 已有

| 能力 | 文档 |
|------|------|
| 五项目分层 + DI | 01 |
| 引擎内核 + 能力接口 | 02 |
| 三租户模板 + Catalog | 03 |
| EF SQLite TPH | 04 |
| 年龄规则 + 检测编排 | 05 |
| AI 插件契约 | 06 |
| REST API（项目/租户/检测） | 07 |
| Vue 首页 + 工作区占位 | 08 |

### 🔶 部分

| 能力 | 缺口 |
|------|------|
| 矛盾检测 | 无 Ollama、无自动触发 |
| 前端 | 无 TipTap、无实体 CRUD |
| 许可证 | 占位校验 |

### ⬜ 未开始（立项必须有）

| 能力 | reference |
|------|-----------|
| TipTap + 手稿树 | 06 |
| WorldCharacter CRUD + @Entity | 03、06 |
| Scrivener 导入 | 03 §八 |
| Ollama 集成 | 05 |
| PWA 安装 / offline | 06、04 |
| Gumroad 正式 license | 07 |
| CI/CD | 11 |

---

## 三、建议迭代顺序（工程）

与 reference/09 里程碑对齐，按 **依赖** 排序：

| 序 | 交付 | 周次（参考） |
|:--:|------|:----------:|
| 1 | EF Migrations + Note CRUD API | 1 |
| 2 | TipTap + ManuscriptTree | 2 |
| 3 | WorldCharacter + EntityLink | 3–4 |
| 4 | TimelineEvent UI | 5–6 |
| 5 | Ollama + WorldBuildingPlugin | 9–10 |
| 6 | PWA + 导入器 | 11–13 |

---

## 四、成功指标（立项，未验证）

来自 reference/09 §五：

| 指标 | 目标 |
|------|------|
| 6 月付费用户 | ≥ 200 |
| NPS | ≥ 40 |
| 矛盾检测周活 | ≥ 60% 付费用户 |

---

## 五、相关文档

- [12-实现状态总表](./12-实现状态总表.md)
- [reference/09-MVP范围与路线图.md](./reference/09-MVP范围与路线图.md)

---

*上一篇：[09-离线存储与同步](./09-离线存储与同步.md) · 下一篇：[11-部署与开发环境](./11-部署与开发环境.md)*
