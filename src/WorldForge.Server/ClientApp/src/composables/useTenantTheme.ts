import { tenantsApi } from '../api/tenants'

let lastTenantId: string | null = null

/** ponytail: 无缓存失效；换租户需再次调用 loadTheme */
export async function loadTenantTheme(tenantId: string): Promise<string> {
  if (lastTenantId === tenantId && document.documentElement.dataset.skin) {
    return document.documentElement.dataset.skin
  }
  const tenant = await tenantsApi.getTenant(tenantId)
  document.documentElement.dataset.skin = tenant.skinId
  lastTenantId = tenantId
  return tenant.skinId
}
