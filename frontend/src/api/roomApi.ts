import type { Room } from "../types/room";

export async function getRooms(): Promise<Room[]> {
    const response = await fetch("/api/room");
    if (!response.ok) {
        throw new Error("Failed to fetch rooms");
    }
    return response.json();
}