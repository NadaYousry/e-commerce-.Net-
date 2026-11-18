import { Link, useRouter } from '../router'
import RequireAdmin from './RequireAdmin'

const SECTIONS = [
  { to: '/admin/products', label: 'Manage products', matches: (pathname) => pathname === '/admin' || pathname.startsWith('/admin/products') },
  { to: '/admin/users', label: 'Manage users', matches: (pathname) => pathname.startsWith('/admin/users') },
]

function AdminDashboard({ children }) {
  const { pathname } = useRouter()

  return (
    <RequireAdmin>
      <div className="dashboard">
        <aside className="panel dashboard-nav">
          <p className="eyebrow dashboard-eyebrow">Dashboard</p>
          <nav aria-label="Dashboard sections">
            {SECTIONS.map((section) => {
              const isActive = section.matches(pathname)
              return (
                <Link
                  key={section.to}
                  to={section.to}
                  className={isActive ? 'dashboard-link active' : 'dashboard-link'}
                  aria-current={isActive ? 'page' : undefined}
                >
                  {section.label}
                </Link>
              )
            })}
          </nav>
        </aside>

        <div className="dashboard-content">{children}</div>
      </div>
    </RequireAdmin>
  )
}

export default AdminDashboard
