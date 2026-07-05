# 07 — 服务端 API

> **立项参考**：[reference/07-后端与可选云服务.md](./reference/07-后端与可选云服务.md)（Auth、Backup、Sync）

---

## 一、宿主配置

**项目**：`WorldForge.Server`  
**框架**：ASP.NET Core 10 Minimal API  
**端口**：`http://localhost:5280`（`appsettings.json` + `launchSettings.json`）

### Program.cs 要点

```csharp
builder.Services.AddWorldForgeCore();
builder.Services.AddWorldForgeInfrastructure($"Data Source={dbPath}");
builder.Services.AddWorldForgeAi();

// Development CORS → http://localhost:5173
await DependencyInjection.MigrateDatabaseAsync(app.Services);

app.MapHealthEndpoints();
app.MapTenantEndpoints();
app.MapProjectEndpoints();
app.MapManuscriptEndpoints();
app.MapContradictionEndpoints();
app.MapLicenseEndpoints();
```

---

## 二、端点清单（已实现）

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/health` | `{ status, version }` |
| GET | `/api/tenants` | 租户摘要列表 |
| GET | `/api/tenants/{tenantId}` | 租户详情 |
| GET | `/api/projects` | 最近项目 |
| POST | `/api/projects` | `{ name }` 创建 |
| GET | `/api/projects/{id}` | 单项目 |
| GET | `/api/projects/{projectId}/manuscripts` | 手稿树 |
| POST | `/api/projects/{projectId}/manuscripts` | 创建章节 |
| GET | `/api/projects/{projectId}/manuscripts/{id}` | 章节详情 + entityLinks |
| PUT | `/api/projects/{projectId}/manuscripts/{id}` | 更新（含全量替换 links） |
| DELETE | `/api/projects/{projectId}/manuscripts/{id}` | 删除（含子树） |
| PATCH | `/api/projects/{projectId}/manuscripts/reorder` | 拖拽排序 |
| POST | `/api/projects/{projectId}/contradictions/detect` | 运行矛盾检测 |
| POST | `/api/license/verify` | `{ licenseKey }` 简易校验 |

实现文件：`Server/Endpoints/ApiEndpoints.cs`（按静态类分组）。

---

## 三、DTO（WorldForge.Shared）

| 类型 | 用途 |
|------|------|
| `ProjectSummaryDto` | Id, Name, LastOpenedAt |
| `CreateProjectRequest` | Name |
| `ContradictionDto` | 检测结果 API 形状 |
| `TenantSummaryDto` / `TenantDetailDto` | 租户 API |
| `LicenseVerifyRequest/Response` | 许可证 |
| `HealthResponse` | 健康检查 |

Server 引用 Shared；Core 实体 **不** 直接暴露给 HTTP。

---

## 四、未实现端点（立项）

| 路径 | 阶段 |
|------|------|
| `/api/auth/*` | Phase 1+ 可选 |
| `/api/backup` | Phase 2 |
| `/api/sync/*` | Phase 2 |
| SignalR `/api/hub` | Phase 2 Yjs |

MVP 可完全离线售卖：reference/07 §2.1 本地 RSA license 校验 — **当前仅长度 ≥8 占位**。

---

## 五、错误与验证

- 创建项目：空名 → `400 Bad Request`  
- 获取项目：不存在 → `404`  
- 矛盾检测：项目不存在 → 空列表（不 404）  

---

## 六、相关文档

- [01-系统全景](./01-系统全景与项目结构.md)
- [08-前端 ClientApp](./08-前端ClientApp.md)

---

## 七、Notes / 手稿 API 规格（待实现）

以下端点供 08 §九–§十二 前端实现，**设计已定、代码未写**。

### 7.1 手稿树

| 方法 | 路径 | 请求 | 响应 |
|------|------|------|------|
| GET | `/api/projects/{projectId}/manuscripts` | — | `ManuscriptNode[]` 扁平或嵌套 |
| POST | `/api/projects/{projectId}/manuscripts` | `{ title, parentId?, sortOrder? }` | `ManuscriptNode` |
| GET | `/api/projects/{projectId}/manuscripts/{id}` | — | `{ id, title, contentJson, status, wordCount, entityLinks[] }` |
| PUT | `/api/projects/{projectId}/manuscripts/{id}` | `{ title?, contentJson?, status?, entityLinks? }` | 204 |
| DELETE | `/api/projects/{projectId}/manuscripts/{id}` | — | 204 |
| PATCH | `/api/projects/{projectId}/manuscripts/reorder` | `{ items: [{ id, parentId, sortOrder }] }` | 204 |

**实现要点**：

- 实体类型固定 `ManuscriptDocument`（Creative 租户）  
- `wordCount`：服务端从 TipTap JSON 纯文本计数  
- `entityLinks` PUT 时 **全量替换** 该文档链接行  

### 7.2 世界构建实体

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/projects/{projectId}/entities` | Query: `type=WorldCharacter` |
| POST | `/api/projects/{projectId}/entities` | Body 含 `discriminator` + 字段 |
| GET | `/api/projects/{projectId}/entities/{id}` | 单实体 |
| PUT | `/api/projects/{projectId}/entities/{id}` | 更新 |
| DELETE | `/api/projects/{projectId}/entities/{id}` | 删除 |

**租户门控**：仅允许 `ITenantTemplate.EntityTypes` 内的 discriminator；否则 `400`。

### 7.3 矛盾列表与状态

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/projects/{projectId}/contradictions` | Query: `status`, `severity` |
| PATCH | `/api/projects/{projectId}/contradictions/{id}` | `{ status: Acknowledged \| Resolved \| FalsePositive }` |

### 7.4 新增 DTO（Shared 规划）

```csharp
public record ManuscriptNodeDto(Guid Id, string Title, Guid? ParentId, int SortOrder, int WordCount, string Status);
public record ManuscriptDetailDto(..., string ContentJson, IReadOnlyList<EntityLinkDto> EntityLinks);
public record EntityLinkDto(Guid TargetEntityId, int StartOffset, int EndOffset);
public record EntitySummaryDto(Guid Id, string Discriminator, string DisplayName);
public record UpsertEntityRequest(string Discriminator, string Title, string? ContentJson, object? Fields);
```

### 7.5 服务层

新增 `INoteService` / `IManuscriptService` / `IEntityService` 于 Core 接口，Infrastructure 实现 — 避免 Endpoint 直接操作 DbContext。

---

*上一篇：[06-AI 插件层](./06-AI插件层.md) · 下一篇：[08-前端 ClientApp](./08-前端ClientApp.md)*
