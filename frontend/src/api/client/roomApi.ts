import type { Room } from "../../types/room";
import { getQuery } from "../../utils/query";

const API_URL = import.meta.env.VITE_API_URL;

export async function getRooms(): Promise<Room[]> {
    const query = getQuery();

    const response = await fetch(`${API_URL}/room${query && "?" + query}`);
    if (!response.ok) {
        throw new Error("Failed to fetch rooms");
    }
    return response.json();
}
