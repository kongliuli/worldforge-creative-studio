export interface EntitySummary {
  id: string
  discriminator: string
  displayName: string
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

export const entitiesApi = {
  list: (projectId: string, type?: string, prefix?: string) => {
    const params = new URLSearchParams()
    if (type) params.set('type', type)
    if (prefix) params.set('prefix', prefix)
    const qs = params.toString()
    return request<EntitySummary[]>(
      `/api/projects/${projectId}/entities${qs ? `?${qs}` : ''}`,
    )
  },

  create: (projectId: string, discriminator: string, title: string) =>
    request<EntitySummary>(`/api/projects/${projectId}/entities`, {
      method: 'POST',
      body: JSON.stringify({ discriminator, title }),
    }),
}
