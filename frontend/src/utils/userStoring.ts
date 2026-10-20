const STORAGE_KEY = "userId";

export function getStoredUserId(): number | null {
    try {
        const stored = localStorage.getItem(STORAGE_KEY);
        return stored ? Number(stored) : null;
    } catch {
        return null;
    }
}

export function storeUserId(value: number) {
    localStorage.setItem(STORAGE_KEY, String(value));
}
