const ARRAY_PARAMS = ["equipment"];
export function buildQuery(filter: object) {
    const params = new URLSearchParams();

    for (const [key, value] of Object.entries(filter)) {
        if (value === undefined || value === null || value === "") continue;

        if (Array.isArray(value)) {
            for (const item of value) {
                if (item !== undefined && item !== null && item !== "") {
                    params.append(toKebab(key), String(item));
                }
            }
        } else if (value instanceof Date) {
            params.append(toKebab(key), value.toISOString());
        } else {
            params.append(toKebab(key), String(value));
        }
    }
    return params.toString();
}

export function getQuery(search: string = window.location.search) {
    const query = parseQuery(search);
    return buildQuery(query);
}

export function parseQuery(search: string = window.location.search) {
    const query: Record<string, string | string[]> = {};
    const sp = new URLSearchParams(search);
    for (const rawKey of new Set(sp.keys())) {
        const key = fromKebab(rawKey);
        if (ARRAY_PARAMS.includes(key)) {
            query[key] = sp.getAll(rawKey);
        } else {
            const value = sp.get(rawKey);
            if (value) query[key] = value;
        }
    }
    return query;
}

const toKebab = (s: string) => s.replace(/[A-Z]/g, (c) => `-${c.toLowerCase()}`);
const fromKebab = (s: string) => s.replace(/-([a-z])/g, (_, c) => c.toUpperCase());
