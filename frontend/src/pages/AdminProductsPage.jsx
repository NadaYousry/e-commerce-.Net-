import { useEffect, useState } from 'react'
import { apiRequest, formatPrice } from '../api'
import AdminDashboard from '../components/AdminDashboard'
import { Link, useRouter } from '../router'
import { useStore } from '../store'

const FLASH_MESSAGES = {
  created: 'Product created.',
  updated: 'Product updated.',
}

function AdminProductsList() {
  const { token } = useStore()
  const { search } = useRouter()
  const [products, setProducts] = useState(null)
  const [status, setStatus] = useState(() => FLASH_MESSAGES[new URLSearchParams(search).get('saved')] ?? '')
  const [statusIsError, setStatusIsError] = useState(false)
  const [deletingId, setDeletingId] = useState(null)

  useEffect(() => {
    apiRequest('/api/admin/products', { token })
      .then(setProducts)
      .catch((error) => {
        setStatus(error.status === 403 ? "Your account doesn't have permission to manage products." : error.message)
        setStatusIsError(true)
        setProducts([])
      })
  }, [token])

  async function handleDelete(product) {
    if (!window.confirm(`Delete "${product.name}"? This can't be undone.`)) {
      return
    }

    setDeletingId(product.id)
    setStatus('')

    try {
      await apiRequest(`/api/admin/products/${product.id}`, { method: 'DELETE', token })
      setProducts((current) => current.filter((item) => item.id !== product.id))
      setStatus(`"${product.name}" was deleted.`)
      setStatusIsError(false)
    } catch (error) {
      setStatus(error.message)
      setStatusIsError(true)
    } finally {
      setDeletingId(null)
    }
  }

  if (!products) {
    return <p className="muted">Loading products...</p>
  }

  return (
    <div className="page">
      {status && (
        <div className={statusIsError ? 'status-box error' : 'status-box'} role="status">{status}</div>
      )}

      <section className="panel">
        <div className="panel-header">
          <div>
            <h2>Manage products</h2>
            <p className="muted admin-subtitle">{products.length} products, including inactive ones.</p>
          </div>
          <Link to="/admin/products/new" className="button-link">+ Add product</Link>
        </div>

        {products.length === 0 ? (
          <p className="empty-state">No products yet. Add your first product to get started.</p>
        ) : (
          <div className="table-scroll">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Product</th>
                  <th>Price</th>
                  <th>Stock</th>
                  <th>Status</th>
                  <th aria-label="Actions" />
                </tr>
              </thead>
              <tbody>
                {products.map((product) => (
                  <tr key={product.id}>
                    <td>
                      <div className="admin-product-cell">
                        <img src={product.imageUrl} alt="" />
                        <div>
                          <strong>{product.name}</strong>
                          <p className="muted">#{product.id}</p>
                        </div>
                      </div>
                    </td>
                    <td>{formatPrice(product.price)}</td>
                    <td className={product.stockQuantity === 0 ? 'out-of-stock' : ''}>{product.stockQuantity}</td>
                    <td>
                      <span className={`status-pill ${product.isActive ? 'status-confirmed' : 'status-cancelled'}`}>
                        {product.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td>
                      <div className="row-actions">
                        <Link to={`/admin/products/${product.id}/edit`} className="button-link small">Edit</Link>
                        <button
                          type="button"
                          className="danger small"
                          onClick={() => handleDelete(product)}
                          disabled={deletingId === product.id}
                        >
                          {deletingId === product.id ? 'Deleting...' : 'Delete'}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}

function AdminProductsPage() {
  return (
    <AdminDashboard>
      <AdminProductsList />
    </AdminDashboard>
  )
}

export default AdminProductsPage
