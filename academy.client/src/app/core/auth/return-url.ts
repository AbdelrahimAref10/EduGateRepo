export function isSafeReturnUrl(url: string | null | undefined): url is string {
  if (!url) return false;
  if (!url.startsWith('/')) return false;
  if (url.startsWith('//') || url.startsWith('/\\')) return false;
  if (url.includes('://')) return false;
  return true;
}

export function loginQueryParams(returnUrl: string | null): Record<string, string> {
  return isSafeReturnUrl(returnUrl) ? { returnUrl } : {};
}

export function studentRegisterQuery(returnUrl?: string | null): Record<string, string> {
  const query: Record<string, string> = { role: 'Student' };
  if (isSafeReturnUrl(returnUrl)) query['returnUrl'] = returnUrl;
  return query;
}
