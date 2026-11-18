function FormField({ label, name, error, hint, required = false, as: Control = 'input', children, ...controlProps }) {
  const errorId = `${name}-error`
  const hintId = `${name}-hint`
  const describedBy = error ? errorId : hint ? hintId : undefined

  return (
    <label className={error ? 'form-field has-error' : 'form-field'}>
      <span>
        {label}
        {required ? <span className="required-mark" aria-hidden="true"> *</span> : <span className="optional-mark"> (optional)</span>}
      </span>
      <Control
        id={name}
        name={name}
        aria-invalid={error ? 'true' : undefined}
        aria-required={required || undefined}
        aria-describedby={describedBy}
        {...controlProps}
      >
        {children}
      </Control>
      {error ? (
        <small id={errorId} className="field-error" role="alert">{error}</small>
      ) : (
        hint && <small id={hintId} className="field-hint">{hint}</small>
      )}
    </label>
  )
}

export default FormField
