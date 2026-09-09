import { useState, type FormEvent } from 'react'
import { addUrl, ApiError } from '../api'

interface Props {
  onAdded: () => void
}

export function AddUrlForm({ onAdded }: Props) {
  const [name, setName] = useState('')
  const [url, setUrl] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError(null)
    setSubmitting(true)

    try {
      await addUrl({ name, url })
      setName('')
      setUrl('')
      onAdded()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Something went wrong')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form className="add-url-form" onSubmit={handleSubmit}>
      <input
        placeholder="Name"
        value={name}
        onChange={(e) => setName(e.target.value)}
        required
      />
      <input
        className="add-url-form__url"
        placeholder="https://example.com"
        value={url}
        onChange={(e) => setUrl(e.target.value)}
        required
      />
      <button type="submit" disabled={submitting}>
        {submitting ? 'Adding…' : 'Add URL'}
      </button>
      {error && <p className="add-url-form__error">{error}</p>}
    </form>
  )
}
