import { createContext, useContext, useEffect, useState } from 'react'

const RouterContext = createContext(null)

function currentLocation() {
  return { pathname: window.location.pathname, search: window.location.search }
}

export function Router({ children }) {
  const [location, setLocation] = useState(currentLocation)

  useEffect(() => {
    const handlePopState = () => setLocation(currentLocation())
    window.addEventListener('popstate', handlePopState)
    return () => window.removeEventListener('popstate', handlePopState)
  }, [])

  function navigate(to, { replace = false } = {}) {
    if (replace) {
      window.history.replaceState(null, '', to)
    } else {
      window.history.pushState(null, '', to)
    }
    setLocation(currentLocation())
    window.scrollTo(0, 0)
  }

  return <RouterContext value={{ ...location, navigate }}>{children}</RouterContext>
}

export function useRouter() {
  return useContext(RouterContext)
}

export function Link({ to, children, ...props }) {
  const { navigate } = useRouter()

  function handleClick(event) {
    // Let the browser handle new-tab / modified clicks.
    if (event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
      return
    }

    event.preventDefault()
    navigate(to)
  }

  return (
    <a href={to} onClick={handleClick} {...props}>
      {children}
    </a>
  )
}
