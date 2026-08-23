export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export const DEFAULT_PAGE_SIZE = 9;

export function emptyPage<T>(page = 1, pageSize = DEFAULT_PAGE_SIZE): PagedResult<T> {
  return {
    items: [],
    totalCount: 0,
    page,
    pageSize,
    totalPages: 0,
    hasPrevious: false,
    hasNext: false,
  };
}

export function mapPagedResult<T>(data: unknown, mapItem: (raw: unknown) => T): PagedResult<T> {
  const d = (typeof data === 'object' && data ? data : {}) as Record<string, unknown>;
  const rawItems = Array.isArray(d['items']) ? d['items'] : [];
  const page = Number(d['page'] ?? 1) || 1;
  const pageSize = Number(d['pageSize'] ?? DEFAULT_PAGE_SIZE) || DEFAULT_PAGE_SIZE;
  const totalCount = Number(d['totalCount'] ?? 0) || 0;
  const totalPages =
    Number(d['totalPages']) ||
    (pageSize > 0 ? Math.ceil(totalCount / pageSize) : 0);

  return {
    items: rawItems.map(mapItem),
    totalCount,
    page,
    pageSize,
    totalPages,
    hasPrevious: Boolean(d['hasPrevious'] ?? page > 1),
    hasNext: Boolean(d['hasNext'] ?? page < totalPages),
  };
}
