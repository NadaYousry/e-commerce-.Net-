import { Link, useRouter } from '../router'
import { useStore } from '../store'

function Layout({ children }) {
  const { user, isAdmin, itemCount, logout } = useStore()
  const { pathname, navigate } = useRouter()

  function navClass(isActive) {
    return isActive ? 'nav-link active' : 'nav-link'
  }

  function handleLogout() {
    logout()
    navigate('/')
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <Link to="/" className="brand">
          <p className="eyebrow">E-Commerce POC</p>
          <h1>E-commerce</h1>
        </Link>

        <nav className="topbar-actions">
          <Link to="/" className={navClass(pathname === '/' || pathname.startsWith('/products'))}>Products</Link>
          <Link to="/cart" className={navClass(pathname === '/cart')}>
            Cart <span className="badge">{itemCount}</span>
          </Link>
          {user ? (
            <>
              <Link to="/orders" className={navClass(pathname.startsWith('/orders'))}>My orders</Link>
              {isAdmin && (
                <Link to="/admin" className={navClass(pathname.startsWith('/admin'))}>Dashboard</Link>
              )}
              <span>Hi, {user.displayName}</span>
              <button type="button" className="secondary" onClick={handleLogout}>Logout</button>
            </>
          ) : (
            <Link to="/login" className={navClass(pathname === '/login' || pathname === '/register')}>Sign in</Link>
          )}
        </nav>
      </header>

      <main>{children}</main>
    </div>
  )
}

export default Layout
