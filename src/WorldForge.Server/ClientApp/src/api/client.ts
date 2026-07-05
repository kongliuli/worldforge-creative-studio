export interface ProjectSummary {
  id: string
  name: string
  lastOpenedAt: string
  tenantTemplateId: string
}

export interface ContradictionItem {
  id: string
  type: string
  severity: string
  summary: string
  status: string
  relatedEntityId: string | null
  suggestion: string | null
}

export interface HealthResponse {
  status: string
  version: string
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
  return res.json() as Promise<T>
}

export const api = {
  health: () => request<HealthResponse>('/api/health'),
  listProjects: () => request<ProjectSummary[]>('/api/projects'),
  createProject: (name: string) =>
    request<ProjectSummary>('/api/projects', {
      method: 'POST',
      body: JSON.stringify({ name }),
    }),
  detectContradictions: (projectId: string) =>
    request<ContradictionItem[]>(`/api/projects/${projectId}/contradictions/detect`, {
      method: 'POST',
    }),
}
