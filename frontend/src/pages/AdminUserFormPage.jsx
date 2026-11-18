import { useState } from 'react'
import { apiRequest } from '../api'
import AdminDashboard from '../components/AdminDashboard'
import FormField from '../components/FormField'
import { Link, useRouter } from '../router'
import { useStore } from '../store'
import { useForm } from '../useForm'
import { PASSWORD_RULES, validateNewUser } from '../validation'

const emptyUser = {
  displayName: '',
  email: '',
  password: '',
  role: 'User',
}

function UserForm() {
  const { token } = useStore()
  const { navigate } = useRouter()
  const { values, errors, handleChange, handleBlur, validateAll, applyServerError } = useForm(emptyUser, validateNewUser)
  const [status, setStatus] = useState('')
  const [saving, setSaving] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    setStatus('')

    if (!validateAll(event.currentTarget)) {
      return
    }

    setSaving(true)

    try {
      await apiRequest('/api/admin/users', {
        method: 'POST',
        token,
        body: {
          displayName: values.displayName.trim(),
          email: values.email.trim(),
          password: values.password,
          role: values.role,
        },
      })

      navigate('/admin/users?saved=created')
    } catch (error) {
      setStatus(applyServerError(error))
      setSaving(false)
    }
  }

  const fieldProps = (name) => ({
    name,
    value: values[name],
    error: errors[name],
    onChange: handleChange,
    onBlur: handleBlur,
  })

  return (
    <form className="panel checkout-panel user-form" onSubmit={handleSubmit} noValidate>
      <h2>Add user</h2>
      <p className="muted form-note">Fields marked <span className="required-mark">*</span> are required.</p>

      <FormField label="Display name" required autoComplete="off" maxLength={60} {...fieldProps('displayName')} />
      <FormField label="Email" type="email" required autoComplete="off" {...fieldProps('email')} />
      <FormField
        label="Temporary password"
        type="password"
        required
        autoComplete="new-password"
        hint="Share it with the user securely."
        {...fieldProps('password')}
      />

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

      <FormField
        label="Role"
        as="select"
        required
        hint={values.role === 'Admin' ? 'Admins can access the dashboard and manage products and users.' : 'Regular users can shop and place orders.'}
        {...fieldProps('role')}
      >
        <option value="User">User</option>
        <option value="Admin">Admin</option>
      </FormField>

      {status && <div className="status-box error" role="alert">{status}</div>}

      <div className="purchase-row">
        <button type="submit" disabled={saving}>{saving ? 'Creating...' : 'Create user'}</button>
        <Link to="/admin/users" className="back-link">Cancel</Link>
      </div>
    </form>
  )
}

function AdminUserFormPage() {
  return (
    <AdminDashboard>
      <div className="page">
        <Link to="/admin/users" className="back-link">← Back to users</Link>
        <UserForm />
      </div>
    </AdminDashboard>
  )
}

export default AdminUserFormPage
