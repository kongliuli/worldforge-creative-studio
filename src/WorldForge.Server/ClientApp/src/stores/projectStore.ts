import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api, type ProjectSummary } from '../api/client'
import { loadProjectList, saveProjectList } from '../utils/projectListCache'

export const useProjectStore = defineStore('project', () => {
  const projects = ref<ProjectSummary[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const fromCache = ref(false)
  const cacheSavedAt = ref<string | null>(null)

  async function loadProjects() {
    loading.value = true
    error.value = null
    fromCache.value = false
    cacheSavedAt.value = null
    try {
      projects.value = await api.listProjects()
      saveProjectList(projects.value)
    } catch (e) {
      const cached = loadProjectList()
      if (cached?.projects.length) {
        projects.value = cached.projects
        fromCache.value = true
        cacheSavedAt.value = cached.savedAt
        error.value = null
      } else {
        error.value =
          e instanceof Error
            ? e.message
            : navigator.onLine
              ? '加载失败'
              : '离线且无缓存 — 请先联网加载项目列表'
      }
    } finally {
      loading.value = false
    }
  }

  async function createProject(name: string) {
    const project = await api.createProject(name)
    projects.value = [project, ...projects.value]
    saveProjectList(projects.value)
    fromCache.value = false
    return project
  }

  return {
    projects,
    loading,
    error,
    fromCache,
    cacheSavedAt,
    loadProjects,
    createProject,
  }
})
