import type { JSONContent } from '@tiptap/core'
import type { EntityLinkDto } from '../api/manuscripts'

export function extractEntityLinks(contentJson: string): EntityLinkDto[] {
  let doc: JSONContent
  try {
    doc = JSON.parse(contentJson) as JSONContent
  } catch {
    return []
  }

  const links: EntityLinkDto[] = []
  let offset = 0

  function walk(node: JSONContent) {
    if (node.type === 'text') {
      offset += node.text?.length ?? 0
      return
    }
    if (node.type === 'entityMention') {
      const label = String(node.attrs?.label ?? '')
      const entityId = String(node.attrs?.entityId ?? '')
      if (entityId) {
        links.push({
          targetEntityId: entityId,
          startOffset: offset,
          endOffset: offset + label.length,
        })
      }
      offset += label.length
      return
    }
    if (node.type === 'hardBreak') {
      offset += 1
      return
    }
    node.content?.forEach(walk)
  }

  walk(doc)
  return links
}
