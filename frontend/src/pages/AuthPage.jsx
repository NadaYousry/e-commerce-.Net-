import { useState } from 'react'
import { apiRequest } from '../api'
import FormField from '../components/FormField'
import { Link, useRouter } from '../router'
import { useStore } from '../store'
import { useForm } from '../useForm'
import { PASSWORD_RULES, validateLogin, validateRegister } from '../validation'

const initialForm = {
  displayName: '',
  email: '',
  password: '',
  confirmPassword: '',
}

function getRedirect(search) {
  const redirect = new URLSearchParams(search).get('redirect')
  // Only allow same-site relative paths.
  return redirect && redirect.startsWith('/') && !redirect.startsWith('//') ? redirect : '/'
}

function AuthPage({ mode }) {
  const { user, signIn } = useStore()
  const { search, navigate } = useRouter()
  const isLogin = mode === 'login'
  const { values, errors, handleChange, handleBlur, validateAll, applyServerError } = useForm(
    isLogin ? { email: '', password: '' } : initialForm,
    isLogin ? validateLogin : validateRegister,
  )
  const [status, setStatus] = useState('')
  const [loading, setLoading] = useState(false)

  async function handleAuth(event) {
    event.preventDefault()
    setStatus('')

    if (!validateAll(event.currentTarget)) {
      return
    }

    setLoading(true)

    try {
      const body = isLogin
        ? { email: values.email.trim(), password: values.password }
        : { displayName: values.displayName.trim(), email: values.email.trim(), password: values.password }

      const data = await apiRequest(`/api/auth/${isLogin ? 'login' : 'register'}`, {
        method: 'POST',
        body,
      })

      signIn(data)
      navigate(getRedirect(search), { replace: true })
    } catch (error) {
      setStatus(applyServerError(error))
      setLoading(false)
    }
  }

  if (user) {
    return (
      <div className="page auth-page">
        <div className="panel auth-summary">
          <h2>Account</h2>
          <p>Signed in as <strong>{user.displayName}</strong></p>
          <p className="muted">{user.email}</p>
          <Link to="/" className="button-link">Browse products</Link>
        </div>
      </div>
    )
  }

  const fieldProps = (name) => ({
    name,
    value: values[name],
    error: errors[name],
    onChange: handleChange,
    onBlur: handleBlur,
  })

  return (
    <div className="page auth-page">
      <form onSubmit={handleAuth} className="panel auth-form" noValidate>
        <h2>{isLogin ? 'Sign in' : 'Create account'}</h2>

        <div className="toggle-row">
          <Link to={`/login${search}`} className={isLogin ? 'active' : ''}>Login</Link>
          <Link to={`/register${search}`} className={isLogin ? '' : 'active'}>Register</Link>
        </div>

        {status && <div className="status-box error" role="alert">{status}</div>}

        {!isLogin && (
          <FormField label="Display name" required autoComplete="name" maxLength={60} {...fieldProps('displayName')} />
        )}

        <FormField label="Email" type="email" required autoComplete="email" {...fieldProps('email')} />

        <FormField
          label="Password"
          type="password"
          required
          autoComplete={isLogin ? 'current-password' : 'new-password'}
          {...fieldProps('password')}
        />

        {!isLogin && (
          <ul className="password-rules" aria-label="Password requirements">
            {PASSWORD_RULES.map((rule) => {
              const met = rule.test(values.password)
              return (
                <li key={rule.label} className={met ? 'met' : ''}>
                  {met ? '✓' : '•'} {rule.label}
                </li>
              )
            })}
          </ul>
        )}

        {!isLogin && (
          <FormField label="Confirm password" type="password" required autoComplete="new-password" {...fieldProps('confirmPassword')} />
        )}

        <button type="submit" disabled={loading}>
          {loading ? 'Please wait...' : isLogin ? 'Login' : 'Create account'}
        </button>
      </form>
    </div>
  )
}

export default AuthPage
