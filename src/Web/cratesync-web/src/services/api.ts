import { appConfig } from '@/config'
import { authService } from '@/services/auth'

export async function apiFetch<T>(
  path: string,
  init?: RequestInit,
): Promise<T> {
  const user = await authService.getUser()

  if (!user || user.expired) {
    throw new Error(
      'No valid access token is available.',
    )
  }

  const response = await fetch(
    `${appConfig.apiBaseUrl}${path}`,
    {
      ...init,
      headers: {
        Accept: 'application/json',
        Authorization:
          `Bearer ${user.access_token}`,
        ...(init?.headers ?? {}),
      },
    },
  )

  if (!response.ok) {
    const problem =
      await response
        .json()
        .catch(() => null)

    const detail =
      problem?.detail ??
      problem?.title ??
      `Request failed with status ${response.status}.`

    throw new Error(detail)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return response.json() as Promise<T>
}
