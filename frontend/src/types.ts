export interface UrlStatus {
  id: number
  name: string
  url: string
  isActive: boolean
  status: number | null // 0 = Up, 1 = Down, null = never checked
  lastStatusCode: number | null
  lastResponseTimeMs: number | null
  lastCheckedAt: string | null
  lastErrorMessage: string | null
}

export interface HistoryEntry {
  checkedAt: string
  statusCode: number | null
  responseTimeMs: number
  status: number // 0 = Up, 1 = Down
  errorMessage: string | null
}

export interface AddUrlRequest {
  name: string
  url: string
}
