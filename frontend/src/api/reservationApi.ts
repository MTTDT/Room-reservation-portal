import type { Reservation, CreateReservation } from "../types/reservation";

export async function getReservations(): Promise<Reservation[]> {
    const response = await fetch("/api/reservation");
    if (!response.ok) {
        throw new Error("Failed to fetch rooms");
    }
    return response.json();
}

export async function createReservation(req: CreateReservation): Promise<Reservation> {
    const response = await fetch("/api/reservation", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(req)
    })
    
    return response.json();
}