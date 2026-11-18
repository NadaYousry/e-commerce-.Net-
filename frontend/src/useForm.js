import { useState } from 'react'

// Client errors appear once a field is blurred or the form is submitted;
// server errors appear immediately and clear when the field is edited.
export function useForm(initialValues, validate) {
  const [values, setValues] = useState(initialValues)
  const [touched, setTouched] = useState({})
  const [submitted, setSubmitted] = useState(false)
  const [serverErrors, setServerErrors] = useState({})

  const clientErrors = validate(values)

  const errors = {}
  for (const name of Object.keys(values)) {
    const clientError = submitted || touched[name] ? clientErrors[name] : undefined
    if (clientError || serverErrors[name]) {
      errors[name] = clientError ?? serverErrors[name]
    }
  }

  function handleChange(event) {
    const { name, value, type, checked } = event.target
    setValues((current) => ({ ...current, [name]: type === 'checkbox' ? checked : value }))
    setServerErrors((current) => {
      if (!current[name]) {
        return current
      }
      const next = { ...current }
      delete next[name]
      return next
    })
  }

  function handleBlur(event) {
    const { name } = event.target
    setTouched((current) => ({ ...current, [name]: true }))
  }

  // Returns true when valid; otherwise reveals all errors and focuses the first invalid field.
  function validateAll(form) {
    setSubmitted(true)
    const firstInvalid = Object.keys(values).find((name) => clientErrors[name])
    if (firstInvalid) {
      form?.elements[firstInvalid]?.focus()
      return false
    }
    return true
  }

  // Maps API field errors onto form fields and returns a message for anything that didn't map.
  function applyServerError(error) {
    const fieldErrors = {}
    const otherMessages = []

    for (const [name, message] of Object.entries(error.fieldErrors ?? {})) {
      if (name in values) {
        fieldErrors[name] = message
      } else {
        otherMessages.push(message)
      }
    }

    setServerErrors(fieldErrors)

    if (otherMessages.length > 0) {
      return otherMessages.join(' ')
    }
    return Object.keys(fieldErrors).length > 0 ? 'Please fix the highlighted fields.' : error.message
  }

  return { values, errors, handleChange, handleBlur, validateAll, applyServerError }
}
