import { formatPrice } from '../api'
import { Link } from '../router'
import { useStore } from '../store'

function CartPage() {
  const { cart, subtotal, itemCount, changeQuantity, removeFromCart } = useStore()

  if (cart.length === 0) {
    return (
      <div className="page">
        <div className="panel">
          <h2>Your cart</h2>
          <p className="empty-state">Your cart is empty.</p>
          <Link to="/" className="button-link">Browse products</Link>
        </div>
      </div>
    )
  }

  return (
    <div className="page two-column">
      <section className="panel cart-panel">
        <div className="panel-header">
          <h2>Your cart</h2>
          <span>{itemCount} items</span>
        </div>

        <div className="cart-items">
          {cart.map((item) => (
            <div key={item.productId} className="cart-item">
              <div className="cart-item-info">
                <Link to={`/products/${item.productId}`}>
                  <img src={item.imageUrl} alt={item.productName} />
                </Link>
                <div>
                  <Link to={`/products/${item.productId}`}><strong>{item.productName}</strong></Link>
                  <p>{formatPrice(item.price)}</p>
                  <button type="button" className="link-button" onClick={() => removeFromCart(item.productId)}>Remove</button>
                </div>
              </div>
              <div className="qty-controls">
                <button type="button" onClick={() => changeQuantity(item.productId, -1)}>-</button>
                <span>{item.quantity}</span>
                <button type="button" onClick={() => changeQuantity(item.productId, 1)}>+</button>
              </div>
              <strong className="line-total">{formatPrice(item.price * item.quantity)}</strong>
            </div>
          ))}
        </div>
      </section>

      <aside className="panel cart-panel">
        <h3>Order summary</h3>
        <div className="summary-row">
          <span>Subtotal</span>
          <strong>{formatPrice(subtotal)}</strong>
        </div>
        <Link to="/checkout" className="button-link checkout-button">Proceed to checkout</Link>
        <Link to="/" className="back-link">Continue shopping</Link>
      </aside>
    </div>
  )
}

export default CartPage
