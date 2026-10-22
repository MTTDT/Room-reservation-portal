import type { Reservation, CreateReservation, UpdateReservation } from "../../types/reservation";
import { getQuery } from "../../utils/query";

const API_URL = import.meta.env.VITE_API_URL;

export async function getReservations(): Promise<Reservation[]> {
    const query = getQuery();

    const response = await fetch(`${API_URL}/reservation${query && "?" + query}`);
    if (!response.ok) {
        throw new Error("Failed to fetch rooms");
    }
    return response.json();
}

export async function createReservation(req: CreateReservation): Promise<Reservation> {
    const response = await fetch(`${API_URL}/reservation`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(req),
    });

    if (!response.ok) {
        const problem = await response.json().catch(() => null);
        throw new Error(problem?.detail ?? `Request failed (${response.status})`);
    }

    return response.json();
}

export async function updateReservation(req: UpdateReservation, id: number): Promise<Reservation> {
    const response = await fetch(`${API_URL}/reservation/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(req),
    });

    if (!response.ok) {
        const problem = await response.json().catch(() => null);
        throw new Error(problem?.detail ?? `Request failed (${response.status})`);
    }
    return response.json();
}
