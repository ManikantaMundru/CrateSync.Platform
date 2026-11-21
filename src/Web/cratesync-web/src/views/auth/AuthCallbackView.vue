<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { CSpinner } from '@coreui/vue'
import { useAuthStore } from '@/stores/auth'
import { authService } from '@/services/auth'

const router = useRouter()
const auth = useAuthStore()
const error = ref<string | null>(null)

onMounted(async () => {
  try {
    await auth.completeLogin()

    await router.replace(
      authService.consumeReturnPath(),
    )
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unable to complete sign in.'
  }
})
</script>

<template>
  <main class="auth-screen">
    <div class="auth-card">
      <template v-if="!error">
        <CSpinner />
        <h1>Signing you in</h1>
        <p>
          Completing your secure CrateSync
          session.
        </p>
      </template>

      <template v-else>
        <h1>Sign-in failed</h1>
        <p>{{ error }}</p>
        <button
          class="btn btn-primary"
          type="button"
          @click="auth.login('/')"
        >
          Try again
        </button>
      </template>
    </div>
  </main>
</template>
