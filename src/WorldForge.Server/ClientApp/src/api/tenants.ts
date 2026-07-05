export interface TenantDetail {
  id: string
  displayName: string
  securityLevel: string
  skinId: string
}

async function request<T>(path: string): Promise<T> {
  const res = await fetch(path)
  if (!res.ok) {
    const text = await res.text()
    throw new Error(text || res.statusText)
  }
  return res.json() as Promise<T>
}

export const tenantsApi = {
  getTenant: (tenantId: string) => request<TenantDetail>(`/api/tenants/${tenantId}`),
}
