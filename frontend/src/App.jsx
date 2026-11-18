import './App.css'
import Layout from './components/Layout'
import AdminProductFormPage from './pages/AdminProductFormPage'
import AdminProductsPage from './pages/AdminProductsPage'
import AdminUserFormPage from './pages/AdminUserFormPage'
import AdminUsersPage from './pages/AdminUsersPage'
import AuthPage from './pages/AuthPage'
import CartPage from './pages/CartPage'
import CheckoutPage from './pages/CheckoutPage'
import HomePage from './pages/HomePage'
import OrderDetailsPage from './pages/OrderDetailsPage'
import OrdersPage from './pages/OrdersPage'
import ProductDetailsPage from './pages/ProductDetailsPage'
import { Link, useRouter } from './router'

function renderPage(pathname) {
  if (pathname === '/') {
    return <HomePage />
  }

  const productMatch = pathname.match(/^\/products\/(\d+)\/?$/)
  if (productMatch) {
    return <ProductDetailsPage key={productMatch[1]} productId={productMatch[1]} />
  }

  const orderMatch = pathname.match(/^\/orders\/(\d+)\/?$/)
  if (orderMatch) {
    return <OrderDetailsPage key={orderMatch[1]} orderId={orderMatch[1]} />
  }

  const adminEditMatch = pathname.match(/^\/admin\/products\/(\d+)\/edit\/?$/)
  if (adminEditMatch) {
    return <AdminProductFormPage key={adminEditMatch[1]} productId={adminEditMatch[1]} />
  }

  switch (pathname) {
    case '/admin':
    case '/admin/products':
      return <AdminProductsPage />
    case '/admin/products/new':
      return <AdminProductFormPage key="new" />
    case '/admin/users':
      return <AdminUsersPage />
    case '/admin/users/new':
      return <AdminUserFormPage />
    case '/orders':
      return <OrdersPage />
    case '/cart':
      return <CartPage />
    case '/checkout':
      return <CheckoutPage />
    case '/login':
      return <AuthPage key="login" mode="login" />
    case '/register':
      return <AuthPage key="register" mode="register" />
    default:
      return (
        <div className="panel">
          <h2>Page not found</h2>
          <Link to="/" className="button-link">Back to products</Link>
        </div>
      )
  }
}

function App() {
  const { pathname } = useRouter()

  return <Layout>{renderPage(pathname)}</Layout>
}

export default App
