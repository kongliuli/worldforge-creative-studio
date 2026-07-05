<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useProjectStore } from '../stores/projectStore'
import { importZip } from '../api/import'

const store = useProjectStore()
const router = useRouter()
const newName = ref('')
const version = ref('')
const importProjectId = ref('')
const importing = ref(false)
const importMessage = ref<string | null>(null)
const offline = ref(!navigator.onLine)

function onOnline() {
  offline.value = false
}
function onOffline() {
  offline.value = true
}

onMounted(async () => {
  window.addEventListener('online', onOnline)
  window.addEventListener('offline', onOffline)
  await store.loadProjects()
  try {
    const health = await fetch('/api/health').then((r) => r.json())
    version.value = health.version
  } catch {
    version.value = 'offline'
  }
})

onUnmounted(() => {
  window.removeEventListener('online', onOnline)
  window.removeEventListener('offline', onOffline)
})

async function createProject() {
  if (!newName.value.trim()) return
  const project = await store.createProject(newName.value.trim())
  newName.value = ''
  await router.push({ name: 'workspace', params: { projectId: project.id } })
}

async function onImportFile(e: Event) {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file || !importProjectId.value) return
  importing.value = true
  importMessage.value = null
  try {
    const result = await importZip(importProjectId.value, file)
    importMessage.value = `导入完成：${result.documentsCreated} 个章节（${result.format}）`
    await store.loadProjects()
  } catch (err) {
    importMessage.value = err instanceof Error ? err.message : '导入失败'
  } finally {
    importing.value = false
    input.value = ''
  }
}
</script>

<template>
  <div class="home">
    <p v-if="offline" class="offline-banner">离线模式 — 部分功能需联网</p>
    <p v-if="store.fromCache" class="offline-banner cache-banner">
      显示离线缓存的项目列表（只读）· 更新于
      {{ store.cacheSavedAt ? new Date(store.cacheSavedAt).toLocaleString() : '未知' }}
    </p>

    <header class="hero">
      <h1>WorldForge</h1>
      <p>离线优先的创作工作室 · 骨架 v{{ version }}</p>
    </header>

    <section class="panel">
      <h2>新建项目</h2>
      <form class="create-form" @submit.prevent="createProject">
        <input v-model="newName" placeholder="项目名称" :disabled="offline" />
        <button type="submit" :disabled="offline">创建</button>
      </form>
      <p v-if="offline" class="muted">离线时无法创建新项目</p>
    </section>

    <section class="panel">
      <h2>导入 ZIP（Markdown / Scrivener）</h2>
      <p class="muted">将 .md 文件或含 .scrivx 的 Scrivener 项目打包为 zip 上传</p>
      <div class="create-form">
        <select v-model="importProjectId">
          <option disabled value="">选择目标项目</option>
          <option v-for="p in store.projects" :key="p.id" :value="p.id">{{ p.name }}</option>
        </select>
        <label class="file-label">
          <input type="file" accept=".zip" :disabled="importing || !importProjectId" @change="onImportFile" />
          {{ importing ? '导入中…' : '选择 ZIP' }}
        </label>
      </div>
      <p v-if="importMessage" class="message">{{ importMessage }}</p>
    </section>

    <section class="panel">
      <h2>最近项目</h2>
      <p v-if="store.loading">加载中…</p>
      <p v-else-if="store.error" class="error">{{ store.error }}</p>
      <p v-else-if="!store.projects.length && offline" class="muted">
        暂无缓存项目 — 请先联网打开一次以缓存列表
      </p>
      <ul v-else-if="store.projects.length" class="project-list">
        <li v-for="p in store.projects" :key="p.id">
          <RouterLink :to="{ name: 'workspace', params: { projectId: p.id } }">
            {{ p.name }}
          </RouterLink>
          <span class="meta">{{ new Date(p.lastOpenedAt).toLocaleString() }}</span>
        </li>
      </ul>
      <p v-else class="muted">暂无项目，创建第一个世界吧。</p>
    </section>
  </div>
</template>
