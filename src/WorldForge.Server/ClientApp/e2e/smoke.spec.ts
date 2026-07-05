import { test, expect } from '@playwright/test'

test('首页加载并创建项目进入工作区', async ({ page }) => {
  await page.goto('/')
  await expect(page.getByRole('heading', { name: 'WorldForge' })).toBeVisible()

  const name = `E2E-${Date.now()}`
  await page.getByPlaceholder('项目名称').fill(name)
  await page.getByRole('button', { name: '创建' }).click()

  await expect(page).toHaveURL(/\/workspace\//)
  await expect(page.getByRole('heading', { name: '工作区' })).toBeVisible()
})
