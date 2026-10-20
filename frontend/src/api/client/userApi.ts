import type { User } from "../../types/user";

const API_URL = import.meta.env.VITE_API_URL;

export async function getUsers(): Promise<User[]> {
    const response = await fetch(`${API_URL}/user`);
    if (!response.ok) {
        throw new Error("Failed to fetch users");
    }
    return response.json();
}
