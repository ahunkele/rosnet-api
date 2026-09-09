import { useEffect, useState } from 'react'
import { getHistory } from '../api'
import type { HistoryEntry } from '../types'
import { statusColor, statusLabel } from '../status'

interface Props {
  urlId: number
}

export function HistoryPanel({ urlId }: Props) {
  const [history, setHistory] = useState<HistoryEntry[] | null>(null)

  useEffect(() => {
    let cancelled = false
    getHistory(urlId).then((data) => {
      if (!cancelled) setHistory(data)
    })
    return () => {
      cancelled = true
    }
  }, [urlId])

  if (history === null) {
    return <div className="history-panel">Loading history…</div>
  }

  if (history.length === 0) {
    return <div className="history-panel">No history yet.</div>
  }

  return (
    <div className="history-panel">
      <table>
        <thead>
          <tr>
            <th>Checked at</th>
            <th>Status</th>
            <th>HTTP code</th>
            <th>Response time</th>
            <th>Error</th>
          </tr>
        </thead>
        <tbody>
          {history.map((h, i) => (
            <tr key={i}>
              <td>{new Date(h.checkedAt).toLocaleString()}</td>
              <td style={{ color: statusColor(h.status) }}>{statusLabel(h.status)}</td>
              <td>{h.statusCode ?? '–'}</td>
              <td>{h.responseTimeMs} ms</td>
              <td className="history-panel__error">{h.errorMessage ?? '–'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
