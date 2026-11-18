import { createContext, useContext, useEffect, useMemo, useState } from 'react'

const StoreContext = createContext(null)

function readJson(key, fallback) {
  const raw = localStorage.getItem(key)
  return raw ? JSON.parse(raw) : fallback
}

export function StoreProvider({ children }) {
  const [cart, setCart] = useState(() => readJson('cart', []))
  const [token, setToken] = useState(localStorage.getItem('token') ?? '')
  const [user, setUser] = useState(() => readJson('user', null))

  useEffect(() => {
    localStorage.setItem('cart', JSON.stringify(cart))
  }, [cart])

  useEffect(() => {
    localStorage.setItem('token', token)
  }, [token])

  useEffect(() => {
    localStorage.setItem('user', JSON.stringify(user))
  }, [user])

  function addToCart(product, quantity = 1) {
    setCart((current) => {
      const existing = current.find((item) => item.productId === product.id)
      if (existing) {
        return current.map((item) =>
          item.productId === product.id
            ? { ...item, quantity: item.quantity + quantity }
            : item,
        )
      }

      return [...current, {
        productId: product.id,
        productName: product.name,
        quantity,
        price: product.price,
        imageUrl: product.imageUrl,
      }]
    })
  }

  function changeQuantity(productId, delta) {
    setCart((current) =>
      current
        .map((item) =>
          item.productId === productId
            ? { ...item, quantity: Math.max(0, item.quantity + delta) }
            : item,
        )
        .filter((item) => item.quantity > 0),
    )
  }

  function removeFromCart(productId) {
    setCart((current) => current.filter((item) => item.productId !== productId))
  }

  function clearCart() {
    setCart([])
  }

  function signIn(data) {
    setToken(data.token)
    setUser({ email: data.email, displayName: data.displayName, roles: data.roles ?? [] })
  }

  function logout() {
    setToken('')
    setUser(null)
  }

  const subtotal = useMemo(
    () => cart.reduce((sum, item) => sum + item.price * item.quantity, 0),
    [cart],
  )

  const itemCount = useMemo(
    () => cart.reduce((sum, item) => sum + item.quantity, 0),
    [cart],
  )

  // UI hint only; the API enforces the Admin role on every admin endpoint.
  const isAdmin = Boolean(user?.roles?.includes('Admin'))

  const value = {
    cart,
    token,
    user,
    isAdmin,
    subtotal,
    itemCount,
    addToCart,
    changeQuantity,
    removeFromCart,
    clearCart,
    signIn,
    logout,
  }

  return <StoreContext value={value}>{children}</StoreContext>
}

export function useStore() {
  return useContext(StoreContext)
}
