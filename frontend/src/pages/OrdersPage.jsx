import { useEffect, useState } from 'react'
import { apiRequest, formatDate, formatPrice } from '../api'
import { Link } from '../router'
import { useStore } from '../store'

export function StatusPill({ value }) {
  return <span className={`status-pill status-${value.toLowerCase()}`}>{value}</span>
}

function OrdersPage() {
  const { token, user } = useStore()
  const [orders, setOrders] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!user) {
      return
    }

    apiRequest('/api/orders', { token })
      .then(setOrders)
      .catch((err) => setError(err.message))
  }, [token, user])

  if (!user) {
    return (
      <div className="page">
        <div className="panel">
          <h2>My orders</h2>
          <p className="muted">Please sign in to view your orders.</p>
          <Link to="/login?redirect=/orders" className="button-link">Sign in</Link>
        </div>
      </div>
    )
  }

  if (error) {
    return <div className="status-box">{error}</div>
  }

  if (!orders) {
    return <p className="muted">Loading orders...</p>
  }

  return (
    <div className="page">
      <section className="panel">
        <div className="panel-header">
          <h2>My orders</h2>
          <span>{orders.length} orders</span>
        </div>

        {orders.length === 0 ? (
          <>
            <p className="empty-state">You haven't placed any orders yet.</p>
            <Link to="/" className="button-link">Browse products</Link>
          </>
        ) : (
          <div className="orders-list">
            {orders.map((order) => (
              <Link key={order.id} to={`/orders/${order.id}`} className="order-row">
                <div>
                  <strong>Order #{order.id}</strong>
                  <p className="muted">{formatDate(order.createdAtUtc)}</p>
                </div>
                <span className="muted">
                  {order.items.reduce((sum, item) => sum + item.quantity, 0)} items
                </span>
                <div className="order-pills">
                  <StatusPill value={order.orderStatus} />
                  <StatusPill value={order.paymentStatus} />
                </div>
                <strong className="line-total">{formatPrice(order.subtotal)}</strong>
              </Link>
            ))}
          </div>
        )}
      </section>
    </div>
  )
}

export default OrdersPage
