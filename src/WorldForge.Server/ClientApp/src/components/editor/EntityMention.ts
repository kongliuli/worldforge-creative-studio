import { Node, mergeAttributes } from '@tiptap/core'
import Suggestion from '@tiptap/suggestion'

export interface EntitySuggestionItem {
  id: string
  type: string
  label: string
}

export type EntitySearchFn = (query: string) => Promise<EntitySuggestionItem[]>

export const EntityMention = Node.create<{ search: EntitySearchFn }>({
  name: 'entityMention',
  group: 'inline',
  inline: true,
  atom: true,

  addOptions() {
    return { search: async () => [] }
  },

  addAttributes() {
    return {
      entityId: { default: null },
      entityType: { default: null },
      label: { default: null },
    }
  },

  parseHTML() {
    return [{ tag: 'span[data-entity-mention]' }]
  },

  renderHTML({ node, HTMLAttributes }) {
    return [
      'span',
      mergeAttributes(HTMLAttributes, {
        'data-entity-mention': '',
        class: 'entity-mention',
      }),
      node.attrs.label ?? '',
    ]
  },

  addProseMirrorPlugins() {
    const search = this.options.search
    return [
      Suggestion({
        editor: this.editor,
        char: '@',
        items: ({ query }) => search(query),
        command: ({ editor, range, props }) => {
          const item = props as EntitySuggestionItem
          editor
            .chain()
            .focus()
            .insertContentAt(range, [
              {
                type: this.name,
                attrs: {
                  entityId: item.id,
                  entityType: item.type,
                  label: item.label,
                },
              },
              { type: 'text', text: ' ' },
            ])
            .run()
        },
        render: () => {
          let popup: HTMLDivElement | null = null
          let pick: (item: EntitySuggestionItem) => void = () => {}

          function paint(items: EntitySuggestionItem[], clientRect?: (() => DOMRect | null) | null) {
            if (!popup) return
            popup.innerHTML = ''
            for (const item of items) {
              const btn = document.createElement('button')
              btn.type = 'button'
              btn.textContent = item.label
              btn.onclick = () => pick(item)
              popup.appendChild(btn)
            }
            const rect = clientRect?.()
            if (rect) {
              popup.style.position = 'fixed'
              popup.style.left = `${rect.left}px`
              popup.style.top = `${rect.bottom + 4}px`
              popup.style.zIndex = '1000'
            }
          }

          return {
            onStart: (props) => {
              pick = props.command
              popup = document.createElement('div')
              popup.className = 'entity-suggestion-popup'
              document.body.appendChild(popup)
              paint(props.items, props.clientRect)
            },
            onUpdate: (props) => {
              pick = props.command
              paint(props.items, props.clientRect)
            },
            onKeyDown: () => false,
            onExit: () => {
              popup?.remove()
              popup = null
            },
          }
        },
      }),
    ]
  },
})
