<script setup lang="ts">
import { computed, ref } from 'vue'
import type { ManuscriptNode } from '../../api/manuscripts'

const props = defineProps<{
  nodes: ManuscriptNode[]
  activeId: string | null
}>()

const emit = defineEmits<{
  select: [id: string]
  create: [parentId: string | null]
  delete: [id: string]
  reorder: [draggedId: string, targetId: string, position: 'before' | 'after' | 'inside']
}>()

interface TreeRow {
  node: ManuscriptNode
  depth: number
}

const draggingId = ref<string | null>(null)
const dropHint = ref<{ id: string; position: 'before' | 'after' | 'inside' } | null>(null)

const rows = computed(() => {
  const byParent = new Map<string | null, ManuscriptNode[]>()
  for (const node of props.nodes) {
    const key = node.parentId
    if (!byParent.has(key)) byParent.set(key, [])
    byParent.get(key)!.push(node)
  }
  for (const list of byParent.values()) list.sort((a, b) => a.sortOrder - b.sortOrder)

  const result: TreeRow[] = []
  function walk(parentId: string | null, depth: number) {
    for (const node of byParent.get(parentId) ?? []) {
      result.push({ node, depth })
      walk(node.id, depth + 1)
    }
  }
  walk(null, 0)
  return result
})

function onDragStart(id: string, e: DragEvent) {
  draggingId.value = id
  e.dataTransfer?.setData('text/plain', id)
  e.dataTransfer!.effectAllowed = 'move'
}

function onDragEnd() {
  draggingId.value = null
  dropHint.value = null
}

function resolvePosition(e: DragEvent): 'before' | 'after' | 'inside' {
  if (e.shiftKey) return 'inside'
  const el = e.currentTarget as HTMLElement
  const rect = el.getBoundingClientRect()
  const y = e.clientY - rect.top
  if (y < rect.height * 0.25) return 'before'
  if (y > rect.height * 0.75) return 'inside'
  return 'after'
}

function onDragOver(id: string, e: DragEvent) {
  e.preventDefault()
  if (!draggingId.value || draggingId.value === id) return
  dropHint.value = { id, position: resolvePosition(e) }
}

function onDrop(id: string, e: DragEvent) {
  e.preventDefault()
  const draggedId = draggingId.value ?? e.dataTransfer?.getData('text/plain')
  if (!draggedId || draggedId === id) return
  emit('reorder', draggedId, id, resolvePosition(e))
  onDragEnd()
}
</script>

<template>
  <ul class="manuscript-tree">
    <li
      v-for="{ node, depth } in rows"
      :key="node.id"
      :class="{
        active: node.id === activeId,
        dragging: node.id === draggingId,
        'drop-before': dropHint?.id === node.id && dropHint.position === 'before',
        'drop-after': dropHint?.id === node.id && dropHint.position === 'after',
        'drop-inside': dropHint?.id === node.id && dropHint.position === 'inside',
      }"
      :style="{ paddingLeft: `${depth * 0.75 + 0.5}rem` }"
      @dragover="onDragOver(node.id, $event)"
      @drop="onDrop(node.id, $event)"
    >
      <span
        class="drag-handle"
        draggable="true"
        title="拖拽排序（Shift=放入子级）"
        @dragstart="onDragStart(node.id, $event)"
        @dragend="onDragEnd"
      >⋮⋮</span>
      <button type="button" class="tree-select" @click="emit('select', node.id)">
        <span class="tree-title">{{ node.title }}</span>
        <span class="tree-meta">{{ node.wordCount }} 字</span>
      </button>
      <button type="button" class="tree-action" title="子章节" @click.stop="emit('create', node.id)">+</button>
      <button type="button" class="tree-action danger" title="删除" @click.stop="emit('delete', node.id)">×</button>
    </li>
  </ul>
</template>
