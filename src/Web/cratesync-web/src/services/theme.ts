export type ThemePreference =
  | 'system'
  | 'light'
  | 'dark'

const storageKey = 'cratesync:theme'
const mediaQuery =
  window.matchMedia('(prefers-color-scheme: dark)')

let preference: ThemePreference =
  readPreference()

function readPreference(): ThemePreference {
  const stored =
    window.localStorage.getItem(storageKey)

  return stored === 'light' ||
    stored === 'dark' ||
    stored === 'system'
    ? stored
    : 'system'
}

function resolveTheme(
  value: ThemePreference,
): 'light' | 'dark' {
  if (value === 'system') {
    return mediaQuery.matches
      ? 'dark'
      : 'light'
  }

  return value
}

function applyTheme(
  value: ThemePreference,
) {
  const resolved = resolveTheme(value)
  const root = document.documentElement

  root.setAttribute(
    'data-coreui-theme',
    resolved,
  )

  root.style.colorScheme = resolved
}

function handleSystemThemeChange() {
  if (preference === 'system')
    applyTheme(preference)
}

export const themeService = {
  initialize(): ThemePreference {
    preference = readPreference()
    applyTheme(preference)

    mediaQuery.addEventListener(
      'change',
      handleSystemThemeChange,
    )

    return preference
  },

  getPreference(): ThemePreference {
    return preference
  },

  setPreference(
    value: ThemePreference,
  ): void {
    preference = value

    window.localStorage.setItem(
      storageKey,
      value,
    )

    applyTheme(value)
  },
}
