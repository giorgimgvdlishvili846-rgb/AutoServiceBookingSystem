const TOKEN_KEY = 'autoservice_token'

export const session = {
  get token() {
    return localStorage.getItem(TOKEN_KEY)
  },
  set token(value) {
    if (value) localStorage.setItem(TOKEN_KEY, value)
    else localStorage.removeItem(TOKEN_KEY)
  },
}

export async function api(path, options = {}) {
  const headers = { ...options.headers }
  if (options.body && !(options.body instanceof FormData)) {
    headers['Content-Type'] = 'application/json'
  }
  if (session.token) headers.Authorization = `Bearer ${session.token}`

  const response = await fetch(`/api${path}`, { ...options, headers })
  if (response.status === 204) return null

  const data = await response.json().catch(() => null)
  if (!response.ok) {
    const validation = data?.errors
      ? Object.values(data.errors).flat().join(' ')
      : null
    throw new Error(validation || data?.message || data?.title || 'Request failed.')
  }
  return data
}
