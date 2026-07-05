import type { ContradictionItem } from './client'

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

export const contradictionsApi = {
  list: (projectId: string, status?: string, severity?: string) => {
    const params = new URLSearchParams()
    if (status) params.set('status', status)
    if (severity) params.set('severity', severity)
    const qs = params.toString()
    return request<ContradictionItem[]>(
      `/api/projects/${projectId}/contradictions${qs ? `?${qs}` : ''}`,
    )
  },

  detect: (projectId: string) =>
    request<ContradictionItem[]>(`/api/projects/${projectId}/contradictions/detect`, {
      method: 'POST',
    }),

  updateStatus: (projectId: string, id: string, status: string) =>
    request<void>(`/api/projects/${projectId}/contradictions/${id}`, {
      method: 'PATCH',
      body: JSON.stringify({ status }),
    }),
}
