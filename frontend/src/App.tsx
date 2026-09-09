import { useCallback, useEffect, useState } from 'react'
import './App.css'
import { getUrls } from './api'
import type { UrlStatus } from './types'
import { AddUrlForm } from './components/AddUrlForm'
import { UrlTable } from './components/UrlTable'

const POLL_INTERVAL_MS = 5000

function App() {
  const [urls, setUrls] = useState<UrlStatus[]>([])
  const [loadError, setLoadError] = useState<string | null>(null)

  const refresh = useCallback(async () => {
    try {
      setUrls(await getUrls())
      setLoadError(null)
    } catch {
      setLoadError('Could not reach the API. Is it running?')
    }
  }, [])

  useEffect(() => {
    refresh()
    const interval = setInterval(refresh, POLL_INTERVAL_MS)
    return () => clearInterval(interval)
  }, [refresh])

  return (
    <div className="app">
      <header className="app__header">
        <h1>URL Health Monitor</h1>
        <p className="app__subtitle">Tracking {urls.length} URL{urls.length === 1 ? '' : 's'}</p>
      </header>

      <AddUrlForm onAdded={refresh} />

      {loadError && <p className="app__error">{loadError}</p>}

      <UrlTable urls={urls} onChanged={refresh} />
    </div>
  )
}

export default App
