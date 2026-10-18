import type { Reservation, CreateReservation, UpdateReservation } from "../types/reservation";

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

    if(!response.ok){
        const problem = await response.json().catch(() => null);
        throw new Error(problem?.detail ?? `Request failed (${response.status})`);
    }
    
    return response.json();
}

export async function updateReservation(req: UpdateReservation, id: number): Promise<Reservation> {
    console.log(req, "<<<")

    const response = await fetch(`/api/reservation/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(req)
    })

        console.log(req, "<<<----")


    if(!response.ok){
        const problem = await response.json().catch(() => null);
        throw new Error(problem?.detail ?? `Request failed (${response.status})`);
    }


    return response.json();
}