export function statusLabel(status: number | null): string {
  if (status === null) return 'Never checked'
  return status === 0 ? 'Up' : 'Down'
}

export function statusColor(status: number | null): string {
  if (status === null) return 'var(--status-unknown)'
  return status === 0 ? 'var(--status-up)' : 'var(--status-down)'
}
