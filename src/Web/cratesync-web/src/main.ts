import { createApp } from 'vue'
import CoreuiVue from '@coreui/vue'
import '@coreui/coreui/dist/css/coreui.min.css'
import '@/styles/app.css'
import App from '@/App.vue'
import { router } from '@/router'
import { pinia } from '@/stores'

createApp(App)
  .use(pinia)
  .use(router)
  .use(CoreuiVue)
  .mount('#app')
