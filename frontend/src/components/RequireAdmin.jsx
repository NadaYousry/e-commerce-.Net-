import { Link, useRouter } from '../router'
import { useStore } from '../store'

function RequireAdmin({ children }) {
  const { user, isAdmin } = useStore()
  const { pathname } = useRouter()

  if (!user) {
    return (
      <div className="page">
        <div className="panel">
          <h2>Dashboard</h2>
          <p className="muted">Please sign in with an admin account.</p>
          <Link to={`/login?redirect=${encodeURIComponent(pathname)}`} className="button-link">Sign in</Link>
        </div>
      </div>
    )
  }

  if (!isAdmin) {
    return (
      <div className="page">
        <div className="panel">
          <h2>Access denied</h2>
          <p className="muted">Your account doesn't have permission to access the dashboard.</p>
          <Link to="/" className="button-link">Back to products</Link>
        </div>
      </div>
    )
  }

  return children
}

export default RequireAdmin
