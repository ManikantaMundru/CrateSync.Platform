import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import type { User } from 'oidc-client-ts'
import { apiFetch } from '@/services/api'
import { authService } from '@/services/auth'
import type { MeResponse } from '@/types/me'

export const useAuthStore =
  defineStore('auth', () => {
    const user = ref<User | null>(null)
    const me = ref<MeResponse | null>(null)
    const initialized = ref(false)
    const loading = ref(false)


    authService.onUserLoaded(
      (loadedUser) => {
        user.value = loadedUser
      },
    )

    const isAuthenticated = computed(
      () =>
        user.value !== null &&
        !user.value.expired,
    )

    const isSuperAdmin = computed(
      () =>
        me.value?.platform.role ===
        'SuperAdmin',
    )

    async function initialize() {
      if (initialized.value)
        return

      loading.value = true

      try {
        user.value =
          await authService.getUser()

        if (
          user.value &&
          !user.value.expired
        ) {
          me.value =
            await apiFetch<MeResponse>(
              '/api/me',
            )
        }
      } finally {
        initialized.value = true
        loading.value = false
      }
    }

    async function refreshMe() {
      me.value =
        await apiFetch<MeResponse>(
          '/api/me',
        )
    }

    async function completeLogin() {
      user.value =
        await authService.completeLogin()

      await refreshMe()
    }

    function login(returnPath = '/') {
      return authService.login(returnPath)
    }

    function logout() {
      return authService.logout()
    }

    function selectTenant(
      tenantId: string,
      returnPath: string,
      asSuperAdmin = false,
    ) {
      authService.beginTenantSelection(
        tenantId,
        asSuperAdmin,
        returnPath,
      )
    }

    function exitTenant(
      returnPath = '/',
    ) {
      authService.exitTenant(returnPath)
    }

    return {
      user,
      me,
      loading,
      initialized,
      isAuthenticated,
      isSuperAdmin,
      initialize,
      refreshMe,
      completeLogin,
      login,
      logout,
      selectTenant,
      exitTenant,
    }
  })
