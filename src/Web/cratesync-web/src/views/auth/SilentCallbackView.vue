<script setup lang="ts">
import { onMounted } from 'vue'
import { authService } from '@/services/auth'

onMounted(async () => {
  await authService.completeSilentLogin()
})
</script>

<template>
  <main class="auth-screen" aria-hidden="true" />
</template>
