export interface ManuscriptNode {
  id: string
  title: string
  parentId: string | null
  sortOrder: number
  wordCount: number
  status: string
}

export interface EntityLinkDto {
  targetEntityId: string
  startOffset: number
  endOffset: number
}

export interface ManuscriptDetail {
  id: string
  title: string
  contentJson: string
  status: string
  wordCount: number
  entityLinks: EntityLinkDto[]
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

export const manuscriptsApi = {
  list: (projectId: string) =>
    request<ManuscriptNode[]>(`/api/projects/${projectId}/manuscripts`),

  get: (projectId: string, id: string) =>
    request<ManuscriptDetail>(`/api/projects/${projectId}/manuscripts/${id}`),

  create: (projectId: string, title: string, parentId?: string | null) =>
    request<ManuscriptNode>(`/api/projects/${projectId}/manuscripts`, {
      method: 'POST',
      body: JSON.stringify({ title, parentId: parentId ?? null }),
    }),

  update: (
    projectId: string,
    id: string,
    body: {
      title?: string
      contentJson?: string
      status?: string
      entityLinks?: EntityLinkDto[]
    },
  ) =>
    request<void>(`/api/projects/${projectId}/manuscripts/${id}`, {
      method: 'PUT',
      body: JSON.stringify(body),
    }),

  delete: (projectId: string, id: string) =>
    request<void>(`/api/projects/${projectId}/manuscripts/${id}`, {
      method: 'DELETE',
    }),

  reorder: (
    projectId: string,
    items: Array<{ id: string; parentId: string | null; sortOrder: number }>,
  ) =>
    request<void>(`/api/projects/${projectId}/manuscripts/reorder`, {
      method: 'PATCH',
      body: JSON.stringify({ items }),
    }),
}
