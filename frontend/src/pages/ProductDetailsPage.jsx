import { useEffect, useState } from 'react'
import { apiRequest, formatPrice } from '../api'
import { Link } from '../router'
import { useStore } from '../store'

function ProductDetailsPage({ productId }) {
  const { addToCart } = useStore()
  const [product, setProduct] = useState(null)
  const [error, setError] = useState('')
  const [quantity, setQuantity] = useState(1)
  const [status, setStatus] = useState('')

  useEffect(() => {
    apiRequest(`/api/products/${productId}`)
      .then(setProduct)
      .catch((err) => setError(err.message))
  }, [productId])

  function handleAdd() {
    addToCart(product, quantity)
    setStatus(`${quantity} × ${product.name} added to cart.`)
    setQuantity(1)
  }

  if (error) {
    return (
      <div className="page">
        <Link to="/" className="back-link">← Back to products</Link>
        <div className="panel">
          <h2>Product not available</h2>
          <p className="muted">{error}</p>
        </div>
      </div>
    )
  }

  if (!product) {
    return <p className="muted">Loading product...</p>
  }

  const outOfStock = product.stockQuantity === 0

  return (
    <div className="page">
      <Link to="/" className="back-link">← Back to products</Link>

      {status && (
        <div className="status-box">
          {status} <Link to="/cart">View cart</Link>
        </div>
      )}

      <section className="panel product-details">
        <img src={product.imageUrl} alt={product.name} />

        <div className="product-info">
          <h2>{product.name}</h2>
          <p className="product-price">{formatPrice(product.price)}</p>
          <p className="muted">{product.description}</p>
          <p>{outOfStock ? 'Out of stock' : `${product.stockQuantity} in stock`}</p>

          {!outOfStock && (
            <div className="purchase-row">
              <div className="qty-controls">
                <button type="button" onClick={() => setQuantity((q) => Math.max(1, q - 1))}>-</button>
                <span>{quantity}</span>
                <button type="button" onClick={() => setQuantity((q) => Math.min(product.stockQuantity, q + 1))}>+</button>
              </div>
              <button type="button" onClick={handleAdd}>Add to cart</button>
            </div>
          )}
        </div>
      </section>
    </div>
  )
}

export default ProductDetailsPage
