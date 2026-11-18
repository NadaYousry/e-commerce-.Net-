export const API_BASE = 'http://localhost:5249'

const priceFormatter = new Intl.NumberFormat('en-EG', { style: 'currency', currency: 'EGP' })

const dateFormatter = new Intl.DateTimeFormat('en-GB', { dateStyle: 'medium', timeStyle: 'short' })

export function formatPrice(value) {
  return priceFormatter.format(value)
}

export function formatDate(value) {
  return dateFormatter.format(new Date(value))
}

export async function apiRequest(path, { token, body, ...options } = {}) {
  const headers = { ...options.headers }

  if (body !== undefined) {
    headers['Content-Type'] = 'application/json'
  }

  if (token) {
    headers.Authorization = `Bearer ${token}`
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  })

  const data = await response.json().catch(() => null)

  if (!response.ok) {
    const error = new Error(
      (typeof data === 'string' && data) || data?.title || data?.message || `Request failed (${response.status}).`,
    )
    error.status = response.status
    error.fieldErrors = toFieldErrors(data?.errors)
    throw error
  }

  return data
}

// ASP.NET ProblemDetails keys look like "Email", "Address.FullName" or "$.paymentMethod";
// reduce them to the camelCase field name used by the forms.
function toFieldErrors(errors) {
  if (!errors || typeof errors !== 'object') {
    return {}
  }

  const result = {}
  for (const [key, messages] of Object.entries(errors)) {
    const last = key.replace(/^\$\.?/, '').split('.').pop().replace(/\[\d+\]$/, '')
    const name = last ? last.charAt(0).toLowerCase() + last.slice(1) : 'form'
    const message = Array.isArray(messages) ? messages.join(' ') : String(messages)
    result[name] = result[name] ? `${result[name]} ${message}` : message
  }
  return result
}
