import type { ProjectSummary } from '../api/client'

const STORAGE_KEY = 'worldforge:project-list'

interface CachedProjectList {
  savedAt: string
  projects: ProjectSummary[]
}

/** ponytail: localStorage 快照；生产环境 Workbox NetworkFirst 另有 SW 缓存 */
export function saveProjectList(projects: ProjectSummary[]): void {
  const payload: CachedProjectList = {
    savedAt: new Date().toISOString(),
    projects,
  }
  localStorage.setItem(STORAGE_KEY, JSON.stringify(payload))
}

export function loadProjectList(): CachedProjectList | null {
  const raw = localStorage.getItem(STORAGE_KEY)
  if (!raw) return null
  try {
    return JSON.parse(raw) as CachedProjectList
  } catch {
    return null
  }
}

export function clearProjectList(): void {
  localStorage.removeItem(STORAGE_KEY)
}
