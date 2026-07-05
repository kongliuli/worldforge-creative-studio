export interface ImportResult {
  documentsCreated: number
  format: string
}

export async function importZip(projectId: string, file: File): Promise<ImportResult> {
  const form = new FormData()
  form.append('file', file)
  const res = await fetch(`/api/projects/${projectId}/import/zip`, {
    method: 'POST',
    body: form,
  })
  if (!res.ok) {
    const text = await res.text()
    throw new Error(text || res.statusText)
  }
  return res.json() as Promise<ImportResult>
}
