export const appConfig = {
  apiBaseUrl:
    import.meta.env.VITE_API_BASE_URL ??
    'https://localhost:7038',
  identityAuthority:
    import.meta.env.VITE_IDENTITY_AUTHORITY ??
    'https://localhost:7201',
}
