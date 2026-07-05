# 08 — 前端 ClientApp

> **立项参考**：[reference/06-前端与编辑器.md](./reference/06-前端与编辑器.md)（TipTap、@Entity、PWA）

---

## 一、技术栈（当前）

| 项 | 选型 |
|----|------|
| 框架 | Vue 3 + TypeScript |
| 构建 | Vite |
| 状态 | Pinia |
| 路由 | Vue Router |
| 端口 | 5173 |
| API | `fetch` → `/api` 代理到 5280 |

路径：`src/WorldForge.Server/ClientApp/`

**未安装**：TipTap、vite-plugin-pwa、@tanstack/vue-virtual。

---

## 二、目录结构

```
ClientApp/src/
├── main.ts              Pinia + Router
├── App.vue              RouterView 壳
├── api/client.ts        REST 封装
├── stores/projectStore.ts
├── router/index.ts
├── views/
│   ├── HomeView.vue     项目列表 + 创建
│   └── WorkspaceView.vue 三栏占位 + 检测按钮
└── style.css            Creative 深色 CSS 变量
```

---

## 三、路由

| 路径 | 组件 | 功能 |
|------|------|------|
| `/` | HomeView | 列项目、创建、health 版本 |
| `/workspace/:projectId` | WorkspaceView | 手稿树/编辑器/矛盾面板占位 |

---

## 四、API 客户端

`api/client.ts`：

- `health()`  
- `listProjects()` / `createProject(name)`  
- `detectContradictions(projectId)`  

代理配置（`vite.config.ts`）：

```typescript
proxy: { '/api': { target: 'http://localhost:5280', changeOrigin: true } }
```

---

## 五、WorkspaceView 布局（占位）

```
┌──────────┬─────────────────┬──────────┐
│ 手稿树    │ TipTap 编辑器    │ 矛盾检测  │
│ 待实现    │ 待实现           │ 按钮可用  │
└──────────┴─────────────────┴──────────┘
```

「运行检测」已调用 `POST .../contradictions/detect` 并展示结果列表。

---

## 六、样式与租户皮肤

`style.css` 硬编码 Creative 变量：

```css
--color-primary: #6366f1;
--color-bg: #0f1117;
/* ... */
```

**待做**（03 §API + 立项 06）：

1. 启动时 `GET /api/tenants/creative`  
2. 按 `skinId` 加载主题（immersive-dark / trust-neutral / clinical-light）  

---

## 七、与立项模块对照

| 立项模块 | 状态 |
|---------|:----:|
| ManuscriptTree | ⬜ |
| TipTapEditor + EntityMention | ⬜ |
| EntityPanel / Graph | ⬜ |
| TimelineView | ⬜ |
| ContradictionPanel | 🔶 过滤 + 确认 |
| SplitEditor | ⬜ |
| PWA offline | 🔶 App Shell |

---

## 八、相关文档

- [07-服务端 API](./07-服务端API.md)
- [reference/06-前端与编辑器.md](./reference/06-前端与编辑器.md)

---

## 九、目标目录结构（待实现）

```
ClientApp/src/
├── api/
│   ├── client.ts
│   ├── projects.ts
│   ├── notes.ts              # Manuscript / Entity CRUD
│   └── contradictions.ts
├── components/
│   ├── editor/
│   │   ├── TipTapEditor.vue
│   │   ├── EntityMention.ts    # TipTap 扩展
│   │   └── EditorToolbar.vue
│   ├── manuscript/
│   │   └── ManuscriptTree.vue
│   ├── entity/
│   │   ├── EntityPanel.vue
│   │   └── EntityAutocomplete.vue
│   └── contradiction/
│       └── ContradictionPanel.vue
├── composables/
│   ├── useTenantTheme.ts       # skinId → CSS 变量
│   └── useAutoSave.ts          # debounce 500ms
├── stores/
│   ├── projectStore.ts
│   ├── manuscriptStore.ts      # 当前章节树 + 选中节点
│   ├── editorStore.ts          # TipTap 实例、dirty 状态
│   └── entityStore.ts
└── themes/
    ├── immersive-dark.css
    ├── trust-neutral.css
    └── clinical-light.css
```

---

## 十、Pinia Store 设计

### manuscriptStore

| 状态 | 类型 | 说明 |
|------|------|------|
| `tree` | `ManuscriptNode[]` | 卷/章层级 |
| `activeDocumentId` | `string \| null` | 当前编辑章节 |
| `loading` | `boolean` | |

| Action | 调用 API |
|--------|---------|
| `loadTree(projectId)` | `GET /api/projects/{id}/manuscripts` |
| `selectDocument(id)` | 本地切换，触发 editor 加载 |
| `reorder(parentId, orderedIds)` | `PATCH .../manuscripts/reorder` |

### editorStore

| 状态 | 说明 |
|------|------|
| `contentJson` | TipTap JSONDocument |
| `isDirty` | 未保存标记 |
| `lastSavedAt` | |

| Action | 行为 |
|--------|------|
| `load(documentId)` | GET 章节 ContentJson |
| `save()` | PUT + 解析 EntityLink（见 §十一） |
| `debouncedSave` | 500ms debounce（reference/04） |

### entityStore

- 按 `entityType` 缓存列表（WorldCharacter 等）  
- `search(prefix)` → 前缀匹配 + 后续语义 API  

---

## 十一、TipTap 与 EntityMention 规格

### 11.1 依赖（待安装）

```
@tiptap/vue-3 @tiptap/starter-kit @tiptap/extension-link
@tiptap/suggestion @tiptap/pm
```

### 11.2 自定义节点 `entityMention`

存储于 `ContentJson`（与 02 §Note 一致）：

```json
{
  "type": "entityMention",
  "attrs": {
    "entityId": "uuid",
    "entityType": "WorldCharacter",
    "label": "暗影领主"
  }
}
```

| 行为 | 规格 |
|------|------|
| 触发 | 输入 `@`，Suggestion 弹层 |
| 数据源 | `entityStore.search` + 前缀；Phase 2 加语义 |
| 渲染 | 内联 span，class `entity-mention` |
| 悬停 | Popover：DisplayName、Age、Faction（IEntityNode 卡片） |
| 点击 | 打开 EntityPanel 或 SplitEditor |

### 11.3 保存时 EntityLink 同步

```
TipTap getJSON()
    → 遍历 entityMention 节点
    → PUT /api/projects/{id}/documents/{docId}
         body: { contentJson, entityLinks: [{ targetEntityId, startOffset, endOffset }] }
    → 服务端替换该文档全部 EntityLink 行
```

偏移算法：ProseMirror `posAtCoords` / 遍历 doc 节点累加 text length。

### 11.4 工具栏 MVP

| 命令 | StarterKit / 扩展 |
|------|------------------|
| H1–H3 | Heading |
| 粗体/斜体 | Bold, Italic |
| 链接 | Link |
| 高亮 | Highlight（可选 extension） |
| `@` | EntityMention |

---

## 十二、ManuscriptTree 规格

| 功能 | MVP | API |
|------|:---:|-----|
| 无限层级展示 | ✅ | 树形 DTO `parentId` + `sortOrder` |
| 单击选中章节 | ✅ | — |
| 拖拽排序 | ✅ | `PATCH reorder` |
| 右键新建/删除 | ✅ | POST / DELETE |
| 节点字数 | ✅ | 服务端 `wordCount` 字段 |
| 虚拟滚动 | 🔶 | `@tanstack/vue-virtual`，>500 节点启用 |

```typescript
interface ManuscriptNode {
  id: string
  title: string
  parentId: string | null
  sortOrder: number
  wordCount: number
  status: 'Draft' | 'Revised' | 'Final'
  children?: ManuscriptNode[]
}
```

---

## 十三、ContradictionPanel 增强规格

在现有列表基础上（已实现 POST 检测）：

| 功能 | 说明 |
|------|------|
| 过滤器 | Severity、Status（Open/Resolved） |
| 跳转 | 点击 → `editorStore.load(sourceDocumentId)` + 高亮 excerpt |
| Split | 双文档 id 时打开 SplitEditor（reference/06 §3.3） |
| 操作 | PATCH status → Acknowledged / FalsePositive |
| 进度 | 全书扫描时显示 `DetectionRunId` + 进度条（Phase 1b） |

所需 API：`GET /api/projects/{id}/contradictions`、`PATCH .../contradictions/{id}`（见 07 §七）。

---

## 十四、租户主题加载

```typescript
// composables/useTenantTheme.ts
const tenant = await api.getTenant(project.tenantTemplateId)
document.documentElement.dataset.skin = tenant.skinId
// 或动态 import(`../themes/${tenant.skinId}.css`)
```

| skinId | 文件 |
|--------|------|
| immersive-dark | 当前 style.css 提取 |
| trust-neutral | themes/trust-neutral.css |
| clinical-light | themes/clinical-light.css |

---

## 十五、路由扩展（规划）

| 路径 | 视图 |
|------|------|
| `/workspace/:projectId` | 写作（默认） |
| `/workspace/:projectId/entities` | 世界构建列表 |
| `/workspace/:projectId/entities/:entityId` | 实体详情 |
| `/workspace/:projectId/timeline` | 时间线 |
| `/workspace/:projectId/settings` | Ollama、自动检测、许可证 |

---

## 十六、Security Level 0（前端）

> 存储与 API 侧见 [04 §十](./04-数据持久化.md)。立项：[reference/08](./reference/08-安全与隐私.md)。

### 16.1 MVP 范围

Creative = Level 0：**无 E2EE UI、无 HIPAA 声明**；设置页须诚实说明数据位置（reference/08 §八）。

### 16.2 TipTap / XSS

| 措施 | 规格 |
|------|------|
| 渲染 | 仅 ProseMirror JSON → DOM；禁用 `v-html` 渲染用户 Markdown |
| 扩展 | StarterKit 默认；Link 协议白名单 `http/https` |
| 粘贴 | `transformPastedHTML`  strip script/on* |
| 导出 | 导出 Markdown 前 sanitize |

### 16.3 CSP（生产构建）

```html
<!-- index.html 或 Server 响应头 — 设计 -->
<meta http-equiv="Content-Security-Policy"
      content="default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; connect-src 'self' http://localhost:11434">
```

| 指令 | 说明 |
|------|------|
| `connect-src` | 含 Ollama `localhost:11434`（若浏览器直连，Phase 2 一般走 Server 代理更安全） |
| nonce | Phase 1b 若内联脚本则改 `script-src 'nonce-…'` |

### 16.4 本地敏感 UI

| 项 | 设计 |
|----|------|
| 许可证密钥 | 不写入 localStorage 明文；可用 sessionStorage 或 OS 凭据 API（Electron） |
| 设置页 | 显示 `ProjectsRoot` 路径、Ollama 仅 localhost 说明 |
| 导出/删除 | 设置页入口（reference/08 清单） |

### 16.5 AI 隐私（UI 文案）

- 矛盾检测默认 **本机 Ollama**；云 AI 模块 Phase 2 **opt-in** 开关，默认关  
- 无内容 telemetry；Crash 可选匿名（reference/08 §六）

### 16.6 实现状态

| 控制 | 状态 |
|------|:----:|
| TipTap sanitize | ⬜ |
| CSP 头 | ⬜ |
| 设置页隐私说明 | ⬜ |
| 导出全部数据 | ⬜ |

---

*上一篇：[07-服务端 API](./07-服务端API.md) · 下一篇：[09-离线存储与同步](./09-离线存储与同步.md)*
