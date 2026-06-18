export function formatResult(result: unknown): string {
  if (process.env.MCP_PRETTY_JSON === "1" || process.env.UNITY_MCP_PRETTY_JSON === "1") {
    return JSON.stringify(result, null, 2);
  }
  return JSON.stringify(result);
}

export interface ResultOptions {
  maxResults?: number;
  offset?: number;
  summaryOnly?: boolean;
  includeDetails?: boolean;
}

function summarizeValue(value: unknown, maxResults: number, offset: number, summaryOnly: boolean): unknown {
  if (Array.isArray(value)) {
    const total = value.length;
    const items = value.slice(offset, offset + maxResults);
    if (summaryOnly) {
      return { total, returned: items.length, offset, truncated: offset + items.length < total };
    }
    return { total, returned: items.length, offset, truncated: offset + items.length < total, items };
  }

  if (value && typeof value === "object") {
    const entries = Object.entries(value as Record<string, unknown>);
    const clone: Record<string, unknown> = {};
    for (const [key, child] of entries) {
      if (Array.isArray(child)) {
        clone[key] = summarizeValue(child, maxResults, offset, summaryOnly);
      } else {
        clone[key] = child;
      }
    }
    return clone;
  }

  return value;
}

export function applyResultOptions(result: unknown, options: ResultOptions = {}): unknown {
  const maxResults = Math.max(1, Math.min(options.maxResults ?? 50, 500));
  const offset = Math.max(0, options.offset ?? 0);
  const summaryOnly = options.summaryOnly === true || options.includeDetails === false;
  if (!options.maxResults && offset === 0 && !summaryOnly) return result;
  return summarizeValue(result, maxResults, offset, summaryOnly);
}

export function textResult(result: unknown, options?: ResultOptions) {
  return { content: [{ type: "text" as const, text: formatResult(applyResultOptions(result, options)) }] };
}
