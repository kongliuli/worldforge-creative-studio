<script setup lang="ts">
import { EditorContent, useEditor } from '@tiptap/vue-3'
import StarterKit from '@tiptap/starter-kit'
import { watch, onBeforeUnmount } from 'vue'
import { entitiesApi } from '../../api/entities'
import { EntityMention } from './EntityMention'

const props = defineProps<{
  projectId: string
  contentJson: string
  disabled?: boolean
}>()

const emit = defineEmits<{
  update: [contentJson: string]
}>()

async function searchEntities(query: string) {
  const list = await entitiesApi.list(props.projectId, 'WorldCharacter', query || undefined)
  return list.map((e) => ({ id: e.id, type: e.discriminator, label: e.displayName }))
}

const editor = useEditor({
  extensions: [
    StarterKit,
    EntityMention.configure({ search: searchEntities }),
  ],
  content: parseContent(props.contentJson),
  editable: !props.disabled,
  onUpdate: ({ editor: e }) => {
    emit('update', JSON.stringify(e.getJSON()))
  },
})

function parseContent(json: string) {
  try {
    return JSON.parse(json)
  } catch {
    return { type: 'doc', content: [{ type: 'paragraph' }] }
  }
}

watch(
  () => props.contentJson,
  (json) => {
    const e = editor.value
    if (!e) return
    const current = JSON.stringify(e.getJSON())
    if (current !== json) e.commands.setContent(parseContent(json), { emitUpdate: false })
  },
)

watch(
  () => props.disabled,
  (disabled) => editor.value?.setEditable(!disabled),
)

onBeforeUnmount(() => editor.value?.destroy())
</script>

<template>
  <div class="tiptap-shell">
    <div v-if="editor" class="tiptap-toolbar">
      <button type="button" :disabled="!editor.can().toggleBold()" @click="editor.chain().focus().toggleBold().run()">
        B
      </button>
      <button type="button" :disabled="!editor.can().toggleItalic()" @click="editor.chain().focus().toggleItalic().run()">
        I
      </button>
      <button type="button" @click="editor.chain().focus().toggleHeading({ level: 2 }).run()">H2</button>
      <span class="toolbar-hint">输入 @ 引用角色</span>
    </div>
    <EditorContent :editor="editor" class="tiptap-editor" />
  </div>
</template>
