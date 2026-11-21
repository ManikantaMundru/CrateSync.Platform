import { createApp } from 'vue'
import '@coreui/coreui/dist/css/coreui.min.css'
import '@coreui/icons/css/free.min.css'
import '@/styles/app.css'
import App from '@/App.vue'
import { router } from '@/router'
import { pinia } from '@/stores'
import { themeService } from '@/services/theme'

themeService.initialize()

createApp(App)
  .use(pinia)
  .use(router)
  .mount('#app')
