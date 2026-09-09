import type { AddUrlRequest, HistoryEntry, UrlStatus } from './types'

const API_BASE = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5044'

export class ApiError extends Error {}

async function parseErrorDetail(res: Response): Promise<string> {
  const problem = await res.json().catch(() => null)
  return problem?.detail ?? `Request failed (${res.status})`
}

export async function getUrls(): Promise<UrlStatus[]> {
  const res = await fetch(`${API_BASE}/api/urls`)
  if (!res.ok) throw new ApiError(await parseErrorDetail(res))
  return res.json()
}

export async function getHistory(id: number): Promise<HistoryEntry[]> {
  const res = await fetch(`${API_BASE}/api/urls/${id}/history`)
  if (!res.ok) throw new ApiError(await parseErrorDetail(res))
  return res.json()
}

export async function addUrl(request: AddUrlRequest): Promise<UrlStatus> {
  const res = await fetch(`${API_BASE}/api/urls`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  })
  if (!res.ok) throw new ApiError(await parseErrorDetail(res))
  return res.json()
}

export async function setActive(id: number, isActive: boolean): Promise<UrlStatus> {
  const res = await fetch(`${API_BASE}/api/urls/${id}/active`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ isActive }),
  })
  if (!res.ok) throw new ApiError(await parseErrorDetail(res))
  return res.json()
}

export async function deleteUrl(id: number): Promise<void> {
  const res = await fetch(`${API_BASE}/api/urls/${id}`, { method: 'DELETE' })
  if (!res.ok) throw new ApiError(await parseErrorDetail(res))
}
