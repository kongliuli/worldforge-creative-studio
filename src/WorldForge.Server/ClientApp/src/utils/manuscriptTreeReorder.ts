import type { ManuscriptNode } from '../api/manuscripts'

export interface ReorderItem {
  id: string
  parentId: string | null
  sortOrder: number
}

export function isDescendant(
  nodes: ManuscriptNode[],
  ancestorId: string,
  nodeId: string,
): boolean {
  let current = nodes.find((n) => n.id === nodeId)
  while (current?.parentId) {
    if (current.parentId === ancestorId) return true
    current = nodes.find((n) => n.id === current!.parentId)
  }
  return false
}

/** position: sibling before/after target, or inside as last child */
export function computeReorder(
  nodes: ManuscriptNode[],
  draggedId: string,
  targetId: string,
  position: 'before' | 'after' | 'inside',
): ReorderItem[] | null {
  if (draggedId === targetId) return null
  if (isDescendant(nodes, draggedId, targetId)) return null

  const copy = nodes.map((n) => ({ ...n }))
  const dragged = copy.find((n) => n.id === draggedId)
  const target = copy.find((n) => n.id === targetId)
  if (!dragged || !target) return null

  const newParent =
    position === 'inside' ? targetId : target.parentId
  dragged.parentId = newParent

  const parents = new Set(copy.map((n) => n.parentId))
  const result: ReorderItem[] = []

  for (const parentId of parents) {
    let siblings = copy
      .filter((n) => n.parentId === parentId)
      .sort((a, b) => a.sortOrder - b.sortOrder)

    siblings = siblings.filter((n) => n.id !== draggedId)

    if (parentId === newParent) {
      let index = siblings.length
      if (position === 'before') {
        const i = siblings.findIndex((s) => s.id === targetId)
        index = i >= 0 ? i : 0
      } else if (position === 'after') {
        const i = siblings.findIndex((s) => s.id === targetId)
        index = i >= 0 ? i + 1 : siblings.length
      }
      siblings.splice(index, 0, dragged)
    }

    siblings.forEach((s, i) => {
      s.sortOrder = i
      result.push({ id: s.id, parentId: s.parentId, sortOrder: i })
    })
  }

  return result
}
