import { useEffect, useState } from 'react'
import { apiRequest } from '../api'
import AdminDashboard from '../components/AdminDashboard'
import { Link, useRouter } from '../router'
import { useStore } from '../store'

const FLASH_MESSAGES = {
  created: 'User created.',
}

function AdminUsersList() {
  const { token, user: currentUser } = useStore()
  const { search } = useRouter()
  const [users, setUsers] = useState(null)
  const [status, setStatus] = useState(() => FLASH_MESSAGES[new URLSearchParams(search).get('saved')] ?? '')
  const [statusIsError, setStatusIsError] = useState(false)
  const [updatingId, setUpdatingId] = useState(null)

  useEffect(() => {
    apiRequest('/api/admin/users', { token })
      .then(setUsers)
      .catch((error) => {
        setStatus(error.status === 403 ? "Your account doesn't have permission to manage users." : error.message)
        setStatusIsError(true)
        setUsers([])
      })
  }, [token])

  function isCurrentUser(user) {
    return user.email.toLowerCase() === currentUser?.email?.toLowerCase()
  }

  async function handleRoleChange(user) {
    const role = user.role === 'Admin' ? 'User' : 'Admin'
    const question = role === 'Admin'
      ? `Give ${user.displayName} admin access to the dashboard?`
      : `Remove admin access from ${user.displayName}?`

    if (!window.confirm(question)) {
      return
    }

    setUpdatingId(user.id)
    setStatus('')

    try {
      const updated = await apiRequest(`/api/admin/users/${user.id}/role`, { method: 'PUT', token, body: { role } })
      setUsers((current) => current.map((item) => (item.id === updated.id ? updated : item)))
      setStatus(`${updated.displayName} is now ${updated.role === 'Admin' ? 'an admin' : 'a regular user'}. The change applies the next time they sign in.`)
      setStatusIsError(false)
    } catch (error) {
      setStatus(error.message)
      setStatusIsError(true)
    } finally {
      setUpdatingId(null)
    }
  }

  if (!users) {
    return <p className="muted">Loading users...</p>
  }

  const adminCount = users.filter((user) => user.role === 'Admin').length

  return (
    <div className="page">
      {status && (
        <div className={statusIsError ? 'status-box error' : 'status-box'} role="status">{status}</div>
      )}

      <section className="panel">
        <div className="panel-header">
          <div>
            <h2>Manage users</h2>
            <p className="muted admin-subtitle">
              {users.length} users, {adminCount} {adminCount === 1 ? 'admin' : 'admins'}.
            </p>
          </div>
          <Link to="/admin/users/new" className="button-link">+ Add user</Link>
        </div>

        {users.length === 0 ? (
          <p className="empty-state">No users yet.</p>
        ) : (
          <div className="table-scroll">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Role</th>
                  <th aria-label="Actions" />
                </tr>
              </thead>
              <tbody>
                {users.map((user) => {
                  const isSelf = isCurrentUser(user)
                  const isAdmin = user.role === 'Admin'
                  return (
                    <tr key={user.id}>
                      <td>
                        <strong>{user.displayName}</strong>
                        {isSelf && <span className="muted"> (you)</span>}
                      </td>
                      <td>{user.email}</td>
                      <td>
                        <span className={`status-pill ${isAdmin ? 'role-admin' : ''}`}>{isAdmin ? 'Admin' : 'User'}</span>
                      </td>
                      <td>
                        <div className="row-actions">
                          <button
                            type="button"
                            className={isAdmin ? 'secondary small' : 'small'}
                            onClick={() => handleRoleChange(user)}
                            disabled={updatingId === user.id || (isSelf && isAdmin)}
                            title={isSelf && isAdmin ? "You can't remove your own admin role." : undefined}
                          >
                            {updatingId === user.id ? 'Saving...' : isAdmin ? 'Make regular user' : 'Make admin'}
                          </button>
                        </div>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}

function AdminUsersPage() {
  return (
    <AdminDashboard>
      <AdminUsersList />
    </AdminDashboard>
  )
}

export default AdminUsersPage
