// Keep these rules in sync with Backend/ECommerce.Api/Services/ValidationRules.cs
// and the DataAnnotations on the request models.
const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
const PHONE_PATTERN = /^\+?\d[\d\s-]{6,19}$/

export const PASSWORD_RULES = [
  { label: 'At least 8 characters', test: (value) => value.length >= 8 },
  { label: 'An uppercase letter', test: (value) => /[A-Z]/.test(value) },
  { label: 'A lowercase letter', test: (value) => /[a-z]/.test(value) },
  { label: 'A number', test: (value) => /\d/.test(value) },
  { label: 'A symbol (e.g. ! @ # $)', test: (value) => /[^a-zA-Z0-9]/.test(value) },
]

function checkText(value, label, { min = 1, max, required = true } = {}) {
  const trimmed = value.trim()
  if (!trimmed) {
    return required ? `${label} is required.` : undefined
  }
  if (trimmed.length < min) {
    return `${label} must be at least ${min} characters.`
  }
  if (max && trimmed.length > max) {
    return `${label} must be ${max} characters or fewer.`
  }
  return undefined
}

function checkEmail(value) {
  if (!value.trim()) {
    return 'Email is required.'
  }
  if (!EMAIL_PATTERN.test(value.trim())) {
    return 'Enter a valid email address, e.g. name@example.com.'
  }
  if (value.trim().length > 256) {
    return 'Email must be 256 characters or fewer.'
  }
  return undefined
}

function compact(errors) {
  return Object.fromEntries(Object.entries(errors).filter(([, message]) => message))
}

export function validateLogin(values) {
  return compact({
    email: checkEmail(values.email),
    password: values.password ? undefined : 'Password is required.',
  })
}

function checkNewPassword(value) {
  const missingRules = PASSWORD_RULES.filter((rule) => !rule.test(value))

  if (!value) {
    return 'Password is required.'
  }
  if (value.length > 100) {
    return 'Password must be 100 characters or fewer.'
  }
  if (missingRules.length > 0) {
    return `Password needs: ${missingRules.map((rule) => rule.label.toLowerCase()).join(', ')}.`
  }
  return undefined
}

export function validateRegister(values) {
  const password = checkNewPassword(values.password)

  let confirmPassword
  if (!values.confirmPassword) {
    confirmPassword = 'Please confirm your password.'
  } else if (values.confirmPassword !== values.password) {
    confirmPassword = 'Passwords do not match.'
  }

  return compact({
    displayName: checkText(values.displayName, 'Display name', { min: 2, max: 60 }),
    email: checkEmail(values.email),
    password,
    confirmPassword,
  })
}

export const USER_ROLES = ['User', 'Admin']

// Mirrors CreateUserRequest in Backend/ECommerce.Api/Controllers/AdminUsersController.cs.
export function validateNewUser(values) {
  return compact({
    displayName: checkText(values.displayName, 'Display name', { min: 2, max: 60 }),
    email: checkEmail(values.email),
    password: checkNewPassword(values.password),
    role: USER_ROLES.includes(values.role) ? undefined : 'Choose a role.',
  })
}

// Mirrors ProductUpsertRequest in Backend/ECommerce.Api/Controllers/AdminProductsController.cs.
export function validateProduct(values) {
  let price
  const priceText = String(values.price).trim()
  if (!priceText) {
    price = 'Price is required.'
  } else if (!/^\d+(\.\d{1,2})?$/.test(priceText)) {
    price = 'Enter a price like 1250 or 1250.50 (up to 2 decimals).'
  } else if (Number(priceText) < 0.01 || Number(priceText) > 1_000_000) {
    price = 'Price must be between 0.01 and 1,000,000.'
  }

  let stockQuantity
  const stockText = String(values.stockQuantity).trim()
  if (!stockText) {
    stockQuantity = 'Stock is required.'
  } else if (!/^\d+$/.test(stockText) || Number(stockText) > 100_000) {
    stockQuantity = 'Stock must be a whole number between 0 and 100,000.'
  }

  let imageUrl
  const urlText = values.imageUrl.trim()
  if (!urlText) {
    imageUrl = 'Image URL is required.'
  } else if (urlText.length > 2048) {
    imageUrl = 'Image URL must be 2048 characters or fewer.'
  } else {
    try {
      const url = new URL(urlText)
      if (url.protocol !== 'http:' && url.protocol !== 'https:') {
        imageUrl = 'Enter a full image URL starting with http:// or https://.'
      }
    } catch {
      imageUrl = 'Enter a full image URL starting with http:// or https://.'
    }
  }

  return compact({
    name: checkText(values.name, 'Name', { min: 2, max: 200 }),
    description: checkText(values.description, 'Description', { min: 10, max: 2000 }),
    price,
    stockQuantity,
    imageUrl,
  })
}

export function validateCheckout(values) {
  let phoneNumber
  if (!values.phoneNumber.trim()) {
    phoneNumber = 'Phone number is required.'
  } else if (!PHONE_PATTERN.test(values.phoneNumber.trim())) {
    phoneNumber = 'Enter a valid phone number (7–20 digits, optional leading +).'
  }

  return compact({
    fullName: checkText(values.fullName, 'Full name', { min: 3, max: 100 }),
    phoneNumber,
    country: checkText(values.country, 'Country', { min: 2, max: 100 }),
    city: checkText(values.city, 'City', { min: 2, max: 100 }),
    streetAddress: checkText(values.streetAddress, 'Street address', { min: 3, max: 200 }),
    buildingApartmentDetails: checkText(values.buildingApartmentDetails, 'Building / apartment', { max: 100, required: false }),
  })
}
