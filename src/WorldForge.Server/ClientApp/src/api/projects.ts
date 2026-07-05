export interface ProjectSettings {
  autoDetectOnSave: boolean
  preferredOllamaModel: string
}

export interface ProjectSummary {
  id: string
  name: string
  lastOpenedAt: string
  tenantTemplateId: string
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(path, {
    headers: { 'Content-Type': 'application/json', ...init?.headers },
    ...init,
  })
  if (!res.ok) {
    const text = await res.text()
    throw new Error(text || res.statusText)
  }
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

export const projectsApi = {
  get: (projectId: string) => request<ProjectSummary>(`/api/projects/${projectId}`),

  getSettings: (projectId: string) =>
    request<ProjectSettings>(`/api/projects/${projectId}/settings`),

  updateSettings: (
    projectId: string,
    settings: { autoDetectOnSave?: boolean; preferredOllamaModel?: string },
  ) =>
    request<void>(`/api/projects/${projectId}/settings`, {
      method: 'PATCH',
      body: JSON.stringify(settings),
    }),
}
