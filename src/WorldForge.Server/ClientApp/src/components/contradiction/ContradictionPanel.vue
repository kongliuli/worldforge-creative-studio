<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { storeToRefs } from 'pinia'
import type { ContradictionItem } from '../../api/client'
import { contradictionsApi } from '../../api/contradictions'
import { useManuscriptStore } from '../../stores/manuscriptStore'

const props = defineProps<{ projectId: string }>()
const manuscriptStore = useManuscriptStore()
const { lastSavedAt } = storeToRefs(manuscriptStore)

const items = ref<ContradictionItem[]>([])
const statusFilter = ref('')
const detecting = ref(false)
const message = ref<string | null>(null)
const loading = ref(false)

async function load() {
  loading.value = true
  try {
    items.value = await contradictionsApi.list(
      props.projectId,
      statusFilter.value || undefined,
    )
  } catch (e) {
    message.value = e instanceof Error ? e.message : '加载失败'
  } finally {
    loading.value = false
  }
}

async function runDetection() {
  detecting.value = true
  message.value = null
  try {
    items.value = await contradictionsApi.detect(props.projectId)
    message.value = `检测完成，发现 ${items.value.length} 条矛盾`
  } catch (e) {
    message.value = e instanceof Error ? e.message : '检测失败'
  } finally {
    detecting.value = false
  }
}

async function acknowledge(id: string) {
  await contradictionsApi.updateStatus(props.projectId, id, 'Acknowledged')
  await load()
}

onMounted(load)
watch(statusFilter, load)
watch(lastSavedAt, (t) => {
  if (t) setTimeout(load, 1500)
})
</script>

<template>
  <div class="contradiction-panel">
    <div class="panel-head">
      <h2>矛盾检测</h2>
      <button :disabled="detecting" type="button" @click="runDetection">
        {{ detecting ? '检测中…' : '运行检测' }}
      </button>
    </div>

    <select v-model="statusFilter" class="filter-select">
      <option value="">全部状态</option>
      <option value="Open">Open</option>
      <option value="Acknowledged">Acknowledged</option>
      <option value="Resolved">Resolved</option>
      <option value="FalsePositive">FalsePositive</option>
    </select>

    <p v-if="message" class="message">{{ message }}</p>
    <p v-if="loading" class="muted">加载中…</p>
    <ul v-else-if="items.length" class="contradiction-list">
      <li v-for="c in items" :key="c.id">
        <strong>{{ c.severity }}</strong> — {{ c.summary }}
        <span class="meta">({{ c.status }})</span>
        <button
          v-if="c.status === 'Open'"
          type="button"
          class="tree-action"
          @click="acknowledge(c.id)"
        >
          确认
        </button>
      </li>
    </ul>
    <p v-else class="muted">暂无矛盾记录</p>
  </div>
</template>
