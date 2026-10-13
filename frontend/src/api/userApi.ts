import type { User } from "../types/user";

export async function getUsers(): Promise<User[]> {
    const response = await fetch("/api/user");
    if (!response.ok) {
        throw new Error("Failed to fetch users");
    }
    return response.json();
}