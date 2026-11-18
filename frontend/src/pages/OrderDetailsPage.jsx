import { useEffect, useState } from 'react'
import { apiRequest, formatDate, formatPrice } from '../api'
import { Link } from '../router'
import { useStore } from '../store'
import { StatusPill } from './OrdersPage'

function OrderDetailsPage({ orderId }) {
  const { token, user } = useStore()
  const [order, setOrder] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!user) {
      return
    }

    apiRequest(`/api/orders/${orderId}`, { token })
      .then(setOrder)
      .catch((err) => setError(err.message))
  }, [orderId, token, user])

  if (!user) {
    return (
      <div className="page">
        <div className="panel">
          <h2>Order #{orderId}</h2>
          <p className="muted">Please sign in to view this order.</p>
          <Link to={`/login?redirect=/orders/${orderId}`} className="button-link">Sign in</Link>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="page">
        <Link to="/orders" className="back-link">← Back to orders</Link>
        <div className="panel">
          <h2>Order not available</h2>
          <p className="muted">{error}</p>
        </div>
      </div>
    )
  }

  if (!order) {
    return <p className="muted">Loading order...</p>
  }

  return (
    <div className="page">
      <Link to="/orders" className="back-link">← Back to orders</Link>

      <div className="two-column">
        <section className="panel cart-panel">
          <div className="panel-header">
            <div>
              <h2>Order #{order.id}</h2>
              <p className="muted">Placed {formatDate(order.createdAtUtc)}</p>
            </div>
            <div className="order-pills">
              <StatusPill value={order.orderStatus} />
              <StatusPill value={order.paymentStatus} />
            </div>
          </div>

          <div className="cart-items">
            {order.items.map((item) => (
              <div key={item.productId} className="cart-item">
                <div>
                  <Link to={`/products/${item.productId}`}><strong>{item.productName}</strong></Link>
                  <p>{item.quantity} × {formatPrice(item.unitPrice)}</p>
                </div>
                <strong>{formatPrice(item.lineTotal)}</strong>
              </div>
            ))}
          </div>

          <div className="summary-row">
            <span>Total</span>
            <strong>{formatPrice(order.subtotal)}</strong>
          </div>
        </section>

        <aside className="page">
          <div className="panel auth-summary">
            <h3>Delivery address</h3>
            <p>{order.fullName}</p>
            <p className="muted">{order.phoneNumber}</p>
            <p className="muted">
              {order.streetAddress}
              {order.buildingApartmentDetails && `, ${order.buildingApartmentDetails}`}
            </p>
            <p className="muted">{order.city}, {order.country}</p>
          </div>

          <div className="panel auth-summary">
            <h3>Payment</h3>
            <p>{order.paymentMethod === 'MockCard' ? 'Mock card' : 'Cash on delivery'}</p>
            <p className="muted">Status: {order.paymentStatus}</p>
          </div>
        </aside>
      </div>
    </div>
  )
}

export default OrderDetailsPage
