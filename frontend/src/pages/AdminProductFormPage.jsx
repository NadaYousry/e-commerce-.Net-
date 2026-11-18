import { useEffect, useState } from 'react'
import { apiRequest } from '../api'
import FormField from '../components/FormField'
import AdminDashboard from '../components/AdminDashboard'
import { Link, useRouter } from '../router'
import { useStore } from '../store'
import { useForm } from '../useForm'
import { validateProduct } from '../validation'

const emptyProduct = {
  name: '',
  description: '',
  price: '',
  stockQuantity: '0',
  imageUrl: '',
  isActive: true,
}

function ProductForm({ productId, initialValues }) {
  const { token } = useStore()
  const { navigate } = useRouter()
  const { values, errors, handleChange, handleBlur, validateAll, applyServerError } = useForm(initialValues, validateProduct)
  const [status, setStatus] = useState('')
  const [saving, setSaving] = useState(false)
  const isEdit = Boolean(productId)

  async function handleSubmit(event) {
    event.preventDefault()
    setStatus('')

    if (!validateAll(event.currentTarget)) {
      return
    }

    setSaving(true)

    try {
      await apiRequest(isEdit ? `/api/admin/products/${productId}` : '/api/admin/products', {
        method: isEdit ? 'PUT' : 'POST',
        token,
        body: {
          name: values.name.trim(),
          description: values.description.trim(),
          price: Number(values.price),
          stockQuantity: Number(values.stockQuantity),
          imageUrl: values.imageUrl.trim(),
          isActive: values.isActive,
        },
      })

      navigate(`/admin/products?saved=${isEdit ? 'updated' : 'created'}`)
    } catch (error) {
      setStatus(applyServerError(error))
      setSaving(false)
    }
  }

  const fieldProps = (name) => ({
    name,
    value: values[name],
    error: errors[name],
    onChange: handleChange,
    onBlur: handleBlur,
  })

  const previewUrl = !errors.imageUrl && /^https?:\/\//.test(values.imageUrl.trim()) ? values.imageUrl.trim() : ''

  return (
    <div className="page two-column">
      <form className="panel checkout-panel" onSubmit={handleSubmit} noValidate>
        <h2>{isEdit ? 'Edit product' : 'Add product'}</h2>
        <p className="muted form-note">Fields marked <span className="required-mark">*</span> are required.</p>

        <FormField label="Name" required maxLength={200} {...fieldProps('name')} />
        <FormField
          label="Description"
          as="textarea"
          rows={4}
          required
          maxLength={2000}
          hint={`${values.description.trim().length}/2000 characters, minimum 10.`}
          {...fieldProps('description')}
        />
        <div className="form-row">
          <FormField label="Price (EGP)" required inputMode="decimal" placeholder="1250.00" {...fieldProps('price')} />
          <FormField label="Stock quantity" required inputMode="numeric" {...fieldProps('stockQuantity')} />
        </div>
        <FormField
          label="Image URL"
          type="url"
          required
          placeholder="https://example.com/image.jpg"
          {...fieldProps('imageUrl')}
        />

        <label className="checkbox-row">
          <input type="checkbox" name="isActive" checked={values.isActive} onChange={handleChange} />
          Active (visible in the store)
        </label>

        {status && <div className="status-box error" role="alert">{status}</div>}

        <div className="purchase-row">
          <button type="submit" disabled={saving}>
            {saving ? 'Saving...' : isEdit ? 'Save changes' : 'Create product'}
          </button>
          <Link to="/admin/products" className="back-link">Cancel</Link>
        </div>
      </form>

      <aside className="panel cart-panel">
        <h3>Preview</h3>
        {previewUrl ? (
          <img className="admin-preview" src={previewUrl} alt="Product preview" />
        ) : (
          <p className="empty-state">Enter a valid image URL to see a preview.</p>
        )}
        <strong>{values.name.trim() || 'Product name'}</strong>
        <p className="muted">{values.description.trim() || 'Product description'}</p>
      </aside>
    </div>
  )
}

function EditProductLoader({ productId }) {
  const { token } = useStore()
  const [product, setProduct] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    apiRequest(`/api/admin/products/${productId}`, { token })
      .then(setProduct)
      .catch((err) => setError(err.status === 404 ? 'This product no longer exists.' : err.message))
  }, [productId, token])

  if (error) {
    return <div className="status-box error">{error}</div>
  }

  if (!product) {
    return <p className="muted">Loading product...</p>
  }

  return (
    <ProductForm
      productId={productId}
      initialValues={{
        name: product.name,
        description: product.description,
        price: String(product.price),
        stockQuantity: String(product.stockQuantity),
        imageUrl: product.imageUrl,
        isActive: product.isActive,
      }}
    />
  )
}

function AdminProductFormPage({ productId }) {
  return (
    <AdminDashboard>
      <div className="page">
        <Link to="/admin/products" className="back-link">← Back to products</Link>
        {productId ? (
          <EditProductLoader productId={productId} />
        ) : (
          <ProductForm initialValues={emptyProduct} />
        )}
      </div>
    </AdminDashboard>
  )
}

export default AdminProductFormPage
