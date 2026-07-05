import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import WorkspaceView from '../views/WorkspaceView.vue'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'home', component: HomeView },
    { path: '/workspace/:projectId', name: 'workspace', component: WorkspaceView, props: true },
  ],
})
