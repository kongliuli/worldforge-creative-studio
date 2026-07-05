import { defineStore } from 'pinia'
import { ref } from 'vue'
import {
  manuscriptsApi,
  type ManuscriptDetail,
  type ManuscriptNode,
} from '../api/manuscripts'
import { extractEntityLinks } from '../utils/extractEntityLinks'
import { computeReorder } from '../utils/manuscriptTreeReorder'

export const useManuscriptStore = defineStore('manuscript', () => {
  const projectId = ref<string | null>(null)
  const tree = ref<ManuscriptNode[]>([])
  const activeDocumentId = ref<string | null>(null)
  const activeDetail = ref<ManuscriptDetail | null>(null)
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)
  const lastSavedAt = ref<number | null>(null)

  async function loadTree(id: string) {
    projectId.value = id
    loading.value = true
    error.value = null
    try {
      tree.value = await manuscriptsApi.list(id)
      if (tree.value.length === 0) {
        const created = await manuscriptsApi.create(id, '第一章')
        tree.value = [created]
      }
      if (!activeDocumentId.value && tree.value.length > 0) {
        await selectDocument(tree.value[0].id)
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : '加载手稿树失败'
    } finally {
      loading.value = false
    }
  }

  async function selectDocument(id: string) {
    if (!projectId.value) return
    activeDocumentId.value = id
    loading.value = true
    error.value = null
    try {
      activeDetail.value = await manuscriptsApi.get(projectId.value, id)
    } catch (e) {
      error.value = e instanceof Error ? e.message : '加载章节失败'
      activeDetail.value = null
    } finally {
      loading.value = false
    }
  }

  async function createDocument(title: string, parentId?: string | null) {
    if (!projectId.value) return
    const node = await manuscriptsApi.create(projectId.value, title, parentId)
    tree.value = await manuscriptsApi.list(projectId.value)
    await selectDocument(node.id)
  }

  async function deleteDocument(id: string) {
    if (!projectId.value) return
    await manuscriptsApi.delete(projectId.value, id)
    tree.value = await manuscriptsApi.list(projectId.value)
    if (activeDocumentId.value === id) {
      activeDocumentId.value = tree.value[0]?.id ?? null
      activeDetail.value = null
      if (activeDocumentId.value) await selectDocument(activeDocumentId.value)
    }
  }

  async function saveContent(contentJson: string) {
    if (!projectId.value || !activeDocumentId.value) return
    saving.value = true
    error.value = null
    try {
      await manuscriptsApi.update(projectId.value, activeDocumentId.value, {
        contentJson,
        entityLinks: extractEntityLinks(contentJson),
      })
      tree.value = await manuscriptsApi.list(projectId.value)
      lastSavedAt.value = Date.now()
    } catch (e) {
      error.value = e instanceof Error ? e.message : '保存失败'
    } finally {
      saving.value = false
    }
  }

  async function reorderDocuments(
    draggedId: string,
    targetId: string,
    position: 'before' | 'after' | 'inside',
  ) {
    if (!projectId.value) return
    const items = computeReorder(tree.value, draggedId, targetId, position)
    if (!items?.length) return

    await manuscriptsApi.reorder(projectId.value, items)
    tree.value = await manuscriptsApi.list(projectId.value)
  }

  return {
    projectId,
    tree,
    activeDocumentId,
    activeDetail,
    loading,
    saving,
    error,
    lastSavedAt,
    loadTree,
    selectDocument,
    createDocument,
    deleteDocument,
    saveContent,
    reorderDocuments,
  }
})
