import {
  UserManager,
  WebStorageStateStore,
  type User,
} from 'oidc-client-ts'
import { appConfig } from '@/config'

const postSessionPathKey =
  'cratesync:post-session-path'

const userManager = new UserManager({
  authority: appConfig.identityAuthority,
  client_id: 'cratesync.web',
  redirect_uri: `${window.location.origin}/auth/callback`,
  post_logout_redirect_uri: `${window.location.origin}/`,
  response_type: 'code',
  scope:
    'openid profile email cratesync.identity cratesync.api',
  automaticSilentRenew: false,
  monitorSession: true,
  loadUserInfo: false,
  userStore: new WebStorageStateStore({
    store: window.sessionStorage,
  }),
})

export const authService = {
  getUser(): Promise<User | null> {
    return userManager.getUser()
  },

  login(returnPath = '/'): Promise<void> {
    sessionStorage.setItem(
      postSessionPathKey,
      returnPath,
    )

    return userManager.signinRedirect()
  },

  async completeLogin(): Promise<User> {
    const user =
      await userManager.signinRedirectCallback()

    return user
  },

  logout(): Promise<void> {
    return userManager.signoutRedirect()
  },

  beginTenantSelection(
    tenantId: string,
    superAdmin: boolean,
    returnPath: string,
  ): void {
    sessionStorage.setItem(
      postSessionPathKey,
      returnPath,
    )

    const callback =
      `${window.location.origin}/auth/session-changed`

    const page = superAdmin
      ? '/Account/EnterTenant'
      : '/Account/SelectTenant'

    const url = new URL(
      page,
      appConfig.identityAuthority,
    )

    url.searchParams.set(
      'SelectedTenantId',
      tenantId,
    )

    url.searchParams.set(
      'ReturnUrl',
      callback,
    )

    window.location.assign(url)
  },

  exitTenant(returnPath = '/'): void {
    sessionStorage.setItem(
      postSessionPathKey,
      returnPath,
    )

    const callback =
      `${window.location.origin}/auth/session-changed`

    const url = new URL(
      '/Account/ExitTenant',
      appConfig.identityAuthority,
    )

    url.searchParams.set(
      'ReturnUrl',
      callback,
    )

    window.location.assign(url)
  },

  refreshAfterSessionChange(): Promise<void> {
    return userManager.signinRedirect()
  },

  consumeReturnPath(): string {
    const path =
      sessionStorage.getItem(
        postSessionPathKey,
      ) ?? '/'

    sessionStorage.removeItem(
      postSessionPathKey,
    )

    return path
  },
}
