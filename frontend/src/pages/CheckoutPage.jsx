import { useState } from 'react'
import { apiRequest, formatPrice } from '../api'
import FormField from '../components/FormField'
import { Link } from '../router'
import { useStore } from '../store'
import { useForm } from '../useForm'
import { validateCheckout } from '../validation'

const initialValues = {
  fullName: '',
  phoneNumber: '',
  country: 'Saudi Arabia',
  city: 'Riyadh',
  streetAddress: '',
  buildingApartmentDetails: '',
  paymentMethod: 'MockCard',
  mockCardSucceeded: true,
}

function CheckoutPage() {
  const { cart, subtotal, token, user, clearCart } = useStore()
  const { values, errors, handleChange, handleBlur, validateAll, applyServerError } = useForm(initialValues, validateCheckout)
  const [status, setStatus] = useState('')
  const [loading, setLoading] = useState(false)
  const [order, setOrder] = useState(null)

  async function handleCheckout(event) {
    event.preventDefault()
    setStatus('')

    if (!validateAll(event.currentTarget)) {
      return
    }

    setLoading(true)

    try {
      const data = await apiRequest('/api/checkout', {
        method: 'POST',
        token,
        body: {
          items: cart.map((item) => ({ productId: item.productId, quantity: item.quantity })),
          address: {
            fullName: values.fullName.trim(),
            phoneNumber: values.phoneNumber.trim(),
            country: values.country.trim(),
            city: values.city.trim(),
            streetAddress: values.streetAddress.trim(),
            buildingApartmentDetails: values.buildingApartmentDetails.trim() || null,
          },
          paymentMethod: values.paymentMethod,
          mockCardSucceeded: values.paymentMethod === 'MockCard' ? values.mockCardSucceeded : undefined,
        },
      })

      setOrder(data)
      clearCart()
    } catch (error) {
      setStatus(error.status === 401 ? 'Your session has expired. Please sign in again.' : applyServerError(error))
    } finally {
      setLoading(false)
    }
  }

  const fieldProps = (name) => ({
    name,
    value: values[name],
    error: errors[name],
    onChange: handleChange,
    onBlur: handleBlur,
  })

  if (order) {
    return (
      <div className="page">
        <div className="panel order-panel">
          <h2>Order placed</h2>
          <p><strong>Order #{order.orderId}</strong></p>
          <p>Status: {order.orderStatus}</p>
          <p>Payment: {order.paymentStatus}</p>
          <p>Total: {formatPrice(order.subtotal)}</p>
          <div className="purchase-row">
            <Link to={`/orders/${order.orderId}`} className="button-link">View order</Link>
            <Link to="/" className="back-link">Continue shopping</Link>
          </div>
        </div>
      </div>
    )
  }

  if (!user) {
    return (
      <div className="page">
        <div className="panel">
          <h2>Checkout</h2>
          <p className="muted">Please sign in before checkout.</p>
          <Link to="/login?redirect=/checkout" className="button-link">Sign in</Link>
        </div>
      </div>
    )
  }

  if (cart.length === 0) {
    return (
      <div className="page">
        <div className="panel">
          <h2>Checkout</h2>
          <p className="empty-state">Your cart is empty.</p>
          <Link to="/" className="button-link">Browse products</Link>
        </div>
      </div>
    )
  }

  return (
    <div className="page two-column">
      <form className="panel checkout-panel" onSubmit={handleCheckout} noValidate>
        <h2>Checkout</h2>
        <p className="muted form-note">Fields marked <span className="required-mark">*</span> are required.</p>

        <FormField label="Full name" required autoComplete="name" maxLength={100} {...fieldProps('fullName')} />
        <FormField
          label="Phone number"
          type="tel"
          required
          autoComplete="tel"
          placeholder="+966 50 000 0000"
          hint="7–20 digits, spaces or dashes allowed, optional leading +."
          {...fieldProps('phoneNumber')}
        />
        <FormField label="Country" required autoComplete="country-name" maxLength={100} {...fieldProps('country')} />
        <FormField label="City" required autoComplete="address-level2" maxLength={100} {...fieldProps('city')} />
        <FormField label="Street address" required autoComplete="address-line1" maxLength={200} {...fieldProps('streetAddress')} />
        <FormField label="Building / apartment" autoComplete="address-line2" maxLength={100} {...fieldProps('buildingApartmentDetails')} />

        <FormField label="Payment method" as="select" required {...fieldProps('paymentMethod')}>
          <option value="CashOnDelivery">Cash on delivery</option>
          <option value="MockCard">Mock card</option>
        </FormField>

        {values.paymentMethod === 'MockCard' && (
          <label className="checkbox-row">
            <input type="checkbox" name="mockCardSucceeded" checked={values.mockCardSucceeded} onChange={handleChange} />
            Mock payment succeeds
          </label>
        )}
        {errors.mockCardSucceeded && <small className="field-error" role="alert">{errors.mockCardSucceeded}</small>}

        {status && <div className="status-box error" role="alert">{status}</div>}

        <button type="submit" className="checkout-button" disabled={loading}>
          {loading ? 'Processing...' : 'Place order'}
        </button>
      </form>

      <aside className="panel cart-panel">
        <div className="panel-header">
          <h3>Order summary</h3>
          <Link to="/cart" className="back-link">Edit cart</Link>
        </div>
        <div className="cart-items">
          {cart.map((item) => (
            <div key={item.productId} className="cart-item">
              <div>
                <strong>{item.productName}</strong>
                <p>{item.quantity} × {formatPrice(item.price)}</p>
              </div>
              <strong>{formatPrice(item.price * item.quantity)}</strong>
            </div>
          ))}
        </div>
        <div className="summary-row">
          <span>Subtotal</span>
          <strong>{formatPrice(subtotal)}</strong>
        </div>
      </aside>
    </div>
  )
}

export default CheckoutPage
