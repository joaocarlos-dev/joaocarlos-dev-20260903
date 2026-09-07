export type EntityStatus = 0 | 1;

export interface ProblemDetails {
  readonly detail?: string;
  readonly errors?: Readonly<Record<string, readonly string[]>>;
  readonly title?: string;
  readonly traceId?: string;
}
