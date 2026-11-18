import { useEffect, useState } from 'react'
import { apiRequest, formatPrice } from '../api'
import { Link } from '../router'
import { useStore } from '../store'

function HomePage() {
  const { addToCart } = useStore()
  const [products, setProducts] = useState([])
  const [status, setStatus] = useState('')

  useEffect(() => {
    apiRequest('/api/products')
      .then(setProducts)
      .catch((error) => {
        setStatus('Unable to load products from the API.')
        console.error(error)
      })
  }, [])

  function handleAdd(product) {
    addToCart(product)
    setStatus(`${product.name} added to cart.`)
  }

  return (
    <div className="page">
      {status && <div className="status-box">{status}</div>}

      <section className="catalog-panel">
        <div className="panel-header">
          <h2>Products</h2>
          <span>{products.length} items</span>
        </div>

        <div className="product-grid">
          {products.map((product) => (
            <article key={product.id} className="product-card">
              <Link to={`/products/${product.id}`}>
                <img src={product.imageUrl} alt={product.name} />
              </Link>
              <div className="product-body">
                <div className="product-title-row">
                  <h3><Link to={`/products/${product.id}`}>{product.name}</Link></h3>
                  <span>{product.stockQuantity} in stock</span>
                </div>
                <p>{product.description}</p>
                <div className="product-footer">
                  <strong>{formatPrice(product.price)}</strong>
                  <button type="button" onClick={() => handleAdd(product)} disabled={product.stockQuantity === 0}>
                    Add to cart
                  </button>
                </div>
              </div>
            </article>
          ))}
        </div>
      </section>
    </div>
  )
}

export default HomePage
