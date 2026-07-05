<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { entitiesApi, type EntitySummary } from '../../api/entities'

const props = defineProps<{ projectId: string }>()

const entities = ref<EntitySummary[]>([])
const loading = ref(false)

async function refresh() {
  loading.value = true
  try {
    entities.value = await entitiesApi.list(props.projectId, 'WorldCharacter')
  } finally {
    loading.value = false
  }
}

async function addCharacter() {
  const name = prompt('角色名称')
  if (!name?.trim()) return
  await entitiesApi.create(props.projectId, 'WorldCharacter', name.trim())
  await refresh()
}

onMounted(refresh)

defineExpose({ refresh })
</script>

<template>
  <div class="entity-panel">
    <div class="panel-head">
      <h3>角色</h3>
      <button type="button" @click="addCharacter">+</button>
    </div>
    <p v-if="loading" class="muted">加载中…</p>
    <ul v-else class="entity-list">
      <li v-for="e in entities" :key="e.id">{{ e.displayName }}</li>
    </ul>
    <p v-if="!loading && !entities.length" class="muted">暂无角色，点击 + 创建</p>
  </div>
</template>
