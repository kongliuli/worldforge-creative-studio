import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import { router } from './router'
import { loadTenantTheme } from './composables/useTenantTheme'
import './style.css'
import './themes/trust-neutral.css'
import './themes/clinical-light.css'

loadTenantTheme('creative').catch(() => {
  document.documentElement.dataset.skin = 'immersive-dark'
})

createApp(App).use(createPinia()).use(router).mount('#app')
