import type { UrlStatus } from '../types'
import { UrlRow } from './UrlRow'

interface Props {
  urls: UrlStatus[]
  onChanged: () => void
}

export function UrlTable({ urls, onChanged }: Props) {
  if (urls.length === 0) {
    return <p className="empty-state">No URLs yet — add one above to start monitoring it.</p>
  }

  return (
    <table className="url-table">
      <thead>
        <tr>
          <th>Name</th>
          <th>URL</th>
          <th>Status</th>
          <th>Last checked</th>
          <th>Response time</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {urls.map((url) => (
          <UrlRow key={url.id} url={url} onChanged={onChanged} />
        ))}
      </tbody>
    </table>
  )
}
