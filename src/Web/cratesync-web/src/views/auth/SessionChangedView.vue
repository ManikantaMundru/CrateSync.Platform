<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { CSpinner } from '@coreui/vue'
import { authService } from '@/services/auth'

const error = ref<string | null>(null)

onMounted(async () => {
  try {
    await authService
      .refreshAfterSessionChange()
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to refresh the session.'
  }
})
</script>

<template>
  <main class="auth-screen">
    <div class="auth-card">
      <template v-if="!error">
        <CSpinner />
        <h1>Updating workspace</h1>
        <p>
          Refreshing your access token with
          your selected organisation.
        </p>
      </template>

      <template v-else>
        <h1>Session update failed</h1>
        <p>{{ error }}</p>
      </template>
    </div>
  </main>
</template>
