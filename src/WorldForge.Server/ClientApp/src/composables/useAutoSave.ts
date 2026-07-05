import { ref } from 'vue'

export function useDebouncedSave(saveFn: (value: string) => void, delayMs = 500) {
  const pending = ref(false)
  let timer: ReturnType<typeof setTimeout> | undefined

  function schedule(value: string) {
    pending.value = true
    clearTimeout(timer)
    timer = setTimeout(() => {
      saveFn(value)
      pending.value = false
    }, delayMs)
  }

  return { schedule, pending }
}
