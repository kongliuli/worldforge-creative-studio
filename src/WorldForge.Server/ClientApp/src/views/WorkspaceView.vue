<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { storeToRefs } from 'pinia'
import ManuscriptTree from '../components/manuscript/ManuscriptTree.vue'
import TipTapEditor from '../components/editor/TipTapEditor.vue'
import EntityPanel from '../components/entity/EntityPanel.vue'
import ContradictionPanel from '../components/contradiction/ContradictionPanel.vue'
import { useDebouncedSave } from '../composables/useAutoSave'
import { loadTenantTheme } from '../composables/useTenantTheme'
import { useManuscriptStore } from '../stores/manuscriptStore'
import { projectsApi } from '../api/projects'

const props = defineProps<{ projectId: string }>()

const manuscriptStore = useManuscriptStore()
const { tree, activeDocumentId, activeDetail, loading, saving, error } =
  storeToRefs(manuscriptStore)

const autoDetectOnSave = ref(false)
const preferredOllamaModel = ref('phi4-mini')
const offline = ref(!navigator.onLine)
const draftJson = ref('{}')

const { schedule: scheduleSave } = useDebouncedSave((json) => {
  manuscriptStore.saveContent(json)
})

onMounted(async () => {
  window.addEventListener('online', () => { offline.value = false })
  window.addEventListener('offline', () => { offline.value = true })
  try {
    const project = await projectsApi.get(props.projectId)
    await loadTenantTheme(project.tenantTemplateId)
  } catch {
    /* ignore */
  }
  await manuscriptStore.loadTree(props.projectId)
  try {
    const settings = await projectsApi.getSettings(props.projectId)
    autoDetectOnSave.value = settings.autoDetectOnSave
    preferredOllamaModel.value = settings.preferredOllamaModel
  } catch {
    /* ignore */
  }
})

async function toggleAutoDetect() {
  await projectsApi.updateSettings(props.projectId, { autoDetectOnSave: autoDetectOnSave.value })
}

async function saveOllamaModel() {
  await projectsApi.updateSettings(props.projectId, {
    preferredOllamaModel: preferredOllamaModel.value.trim(),
  })
}

watch(activeDetail, (detail) => {
  draftJson.value = detail?.contentJson ?? '{"type":"doc","content":[{"type":"paragraph"}]}'
})

function onEditorUpdate(json: string) {
  draftJson.value = json
  scheduleSave(json)
}

async function addRootChapter() {
  const n = tree.value.length + 1
  await manuscriptStore.createDocument(`第 ${n} 章`)
}

async function addChildChapter(parentId: string | null) {
  if (!parentId) return
  await manuscriptStore.createDocument('新章节', parentId)
}

async function removeChapter(id: string) {
  if (!confirm('删除该章节及其子章节？')) return
  await manuscriptStore.deleteDocument(id)
}
</script>

<template>
  <div class="workspace">
    <header class="workspace-header">
      <RouterLink to="/">← 项目列表</RouterLink>
      <h1>工作区</h1>
      <span class="meta">{{ offline ? '离线（只读）' : saving ? '保存中…' : '已同步' }}</span>
      <label class="auto-detect-toggle">
        <input v-model="autoDetectOnSave" type="checkbox" @change="toggleAutoDetect" />
        保存时检测
      </label>
      <label class="ollama-model">
        Ollama
        <input
          v-model="preferredOllamaModel"
          type="text"
          placeholder="phi4-mini"
          @change="saveOllamaModel"
        />
      </label>
    </header>

    <p v-if="offline" class="offline-banner">离线模式 — 编辑已禁用，待联网后同步（PWA-2）</p>
    <p v-if="error" class="error">{{ error }}</p>

    <div class="workspace-grid">
      <aside class="sidebar panel">
        <div class="panel-head">
          <h2>手稿树</h2>
          <button type="button" @click="addRootChapter">+ 章节</button>
        </div>
        <p v-if="loading && !tree.length" class="muted">加载中…</p>
        <ManuscriptTree
          v-else
          :nodes="tree"
          :active-id="activeDocumentId"
          @select="manuscriptStore.selectDocument"
          @create="addChildChapter"
          @delete="removeChapter"
          @reorder="(d, t, p) => manuscriptStore.reorderDocuments(d, t, p)"
        />
      </aside>

      <main class="editor panel">
        <h2 v-if="activeDetail">{{ activeDetail.title }}</h2>
        <p v-else-if="loading" class="muted">加载章节…</p>
        <TipTapEditor
          v-if="activeDetail"
          :project-id="projectId"
          :content-json="draftJson"
          :disabled="loading || offline"
          @update="onEditorUpdate"
        />
        <EntityPanel :project-id="projectId" />
      </main>

      <aside class="right panel">
        <ContradictionPanel :project-id="projectId" />
      </aside>
    </div>
  </div>
</template>
