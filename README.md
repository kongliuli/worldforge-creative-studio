# WorldForge — 创意工作者一体化创作工作室

> **离线优先的长篇小说 + TTRPG 世界构建工作室，带本地 AI 矛盾检测。**

**产品代号**：WorldForge  
**优先级**：P0 立即启动  
**技术架构文档**：见 [`docs/design/`](./docs/design/)（实际架构）；立项参考见 [`docs/design/reference/`](./docs/design/reference/)

---

## 一句话定位

Scrivener 的现代替代品 + World Anvil 级世界构建 + PlotForge/Ishvana 级本地 AI 矛盾检测，$69–99 一次性买断，数据完全本地化。

---

## 快速启动（骨架）

```bash
# 后端 API（http://localhost:5280）
dotnet watch run --project src/WorldForge.Server

# 前端 PWA 壳（另开终端，http://localhost:5173）
cd src/WorldForge.Server/ClientApp && npm run dev

# 测试
dotnet test
```

### 解决方案结构（复用架构）

```
src/WorldForge.Core/
├── Abstractions/          # 跨租户能力接口（IEntityNode、IContradictionRule…）
├── Entities/              # 共享内核（Note、Project、Contradiction、EntityRelationship）
├── Tenants/
│   ├── Creative/          # WorldForge MVP 实体 + 模板
│   ├── Legacy/            # LegacyVault 占位（B 场景）
│   └── Health/            # HealthVault 占位（C 场景）
├── Rules/                 # 可注册矛盾规则
└── DependencyInjection.cs # 租户注册表 + 规则注册

src/WorldForge.Infrastructure/  # 租户无关编排（ContradictionService 读 TenantTemplateId）
src/WorldForge.AI/                # 插件实现（IContradictionDetectionPlugin）
src/WorldForge.Server/ClientApp/  # 按 tenant.SkinId 换肤（待接 /api/tenants）
```

**扩展新垂直场景**：新增 `Tenants/Xxx/` 实体 + `XxxTenantTemplate` → 注册到 `AddWorldForgeCore()` → 可选 AI 插件 + 规则。无需改 DbContext 编排逻辑。

---

## 文档导航

→ **[docs/README.md](./docs/README.md)** 完整索引

| 类型 | 入口 |
|------|------|
| **实际架构**（01–12，对齐代码） | [design/00-阅读指南](./docs/design/00-阅读指南.md) |
| **立项参考**（原调研提炼） | [design/reference/](./docs/design/reference/) |

---

## 技术栈速查

| 层级 | 选型 |
|------|------|
| 后端 | ASP.NET Core 10 + PostgreSQL（可选云备份） |
| 前端 | Vue3 + TipTap PWA |
| 本地存储 | SQLite（客户端权威） |
| AI | Ollama + ONNX Runtime（设备端） |
| 向量搜索 | EF Core `VectorDistance` |
| 同步 | Yjs CRDT（Phase 2） |
| 导出 | QuestPDF / EPUB 库（Phase 2） |
| 桌面 | PWA 优先；Electron 备选 |

---

## 源调研文档

本架构文档集基于调研产出；**实施以 `docs/design/01–12` 为准**，背景见 [reference/00-阅读指南-立项参考.md](./docs/design/reference/00-阅读指南-立项参考.md)：

- `0705调研报告/AI笔记垂直场景/A-创意工作者工作室.md`
- `ai笔记调研项目图景/` 下场景6深度调研与架构复用文档

---

*生成时间：2026-07-05*
