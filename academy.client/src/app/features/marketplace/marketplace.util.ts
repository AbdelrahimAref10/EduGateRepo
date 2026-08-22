export function personInitials(name?: string | null): string {
  if (!name?.trim()) return '?';
  const parts = name.trim().split(/\s+/).filter(Boolean);
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
}

export function ratingFromReviews(reviews?: { rating?: number | null }[] | null): { average: number; count: number } {
  const values = (reviews ?? [])
    .map((review) => Number(review.rating))
    .filter((rating) => Number.isFinite(rating) && rating > 0);

  if (!values.length) return { average: 0, count: 0 };

  const average = values.reduce((sum, rating) => sum + rating, 0) / values.length;
  return { average, count: values.length };
}

export function resolvedRating(
  average?: number | null,
  count?: number | null,
  reviews?: { rating?: number | null }[] | null,
): { average: number; count: number } {
  const summaryCount = Number(count ?? 0);
  const summaryAverage = Number(average ?? 0);
  if (summaryCount > 0 && summaryAverage > 0) {
    return { average: summaryAverage, count: summaryCount };
  }

  const fromReviews = ratingFromReviews(reviews);
  if (fromReviews.count > 0) return fromReviews;

  return { average: summaryAverage, count: summaryCount };
}

export function ratingLabel(
  average?: number | null,
  count?: number | null,
  reviews?: { rating?: number | null }[] | null,
): string {
  const rating = resolvedRating(average, count, reviews);
  if (!rating.count) return '';
  return `${rating.average.toFixed(1)} (${rating.count})`;
}

export function filledStars(
  average?: number | null,
  count?: number | null,
  stars?: number | null,
  reviews?: { rating?: number | null }[] | null,
): number {
  const rating = resolvedRating(average, count, reviews);
  if (typeof stars === 'number' && Number.isFinite(stars) && stars > 0 && rating.count > 0) {
    return Math.min(5, Math.max(1, Math.round(stars)));
  }

  if (!rating.count) return 0;
  return Math.min(5, Math.max(1, Math.round(rating.average)));
}
