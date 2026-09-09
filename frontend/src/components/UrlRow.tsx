import { useState } from 'react'
import { deleteUrl, setActive } from '../api'
import type { UrlStatus } from '../types'
import { statusColor, statusLabel } from '../status'
import { HistoryPanel } from './HistoryPanel'

interface Props {
  url: UrlStatus
  onChanged: () => void
}

export function UrlRow({ url, onChanged }: Props) {
  const [historyOpen, setHistoryOpen] = useState(false)
  const [busy, setBusy] = useState(false)

  const togglePause = async () => {
    setBusy(true)
    try {
      await setActive(url.id, !url.isActive)
      onChanged()
    } finally {
      setBusy(false)
    }
  }

  const handleDelete = async () => {
    setBusy(true)
    try {
      await deleteUrl(url.id)
      onChanged()
    } finally {
      setBusy(false)
    }
  }

  return (
    <>
      <tr className={url.isActive ? 'url-row' : 'url-row url-row--paused'}>
        <td>{url.name}</td>
        <td className="url-row__url">{url.url}</td>
        <td>
          <span className="status-badge" style={{ color: statusColor(url.status) }}>
            ● {url.isActive ? statusLabel(url.status) : 'Paused'}
          </span>
        </td>
        <td>{url.lastCheckedAt ? new Date(url.lastCheckedAt).toLocaleTimeString() : '–'}</td>
        <td>{url.lastResponseTimeMs !== null ? `${url.lastResponseTimeMs} ms` : '–'}</td>
        <td className="url-row__actions">
          <button onClick={() => setHistoryOpen((open) => !open)}>
            {historyOpen ? 'Hide history' : 'History'}
          </button>
          <button onClick={togglePause} disabled={busy}>
            {url.isActive ? 'Pause' : 'Resume'}
          </button>
          <button className="button--danger" onClick={handleDelete} disabled={busy}>
            Delete
          </button>
        </td>
      </tr>
      {historyOpen && (
        <tr>
          <td colSpan={6} className="url-row__history-cell">
            <HistoryPanel urlId={url.id} />
          </td>
        </tr>
      )}
    </>
  )
}
