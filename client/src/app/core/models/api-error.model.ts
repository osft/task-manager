export interface ApiErrorBody {
  error?: string;
}

export function readApiError(error: unknown): string {
  if (typeof error === 'object' && error !== null && 'error' in error) {
    const body = error as ApiErrorBody;
    if (typeof body.error === 'string' && body.error.length > 0) {
      return body.error;
    }
  }

  return 'Something went wrong. Please try again.';
}
