import type { Equipment } from "./equipment";
export interface Room {
    id: number;
    name: string;
    capacity: number;
    status: "Active" | "Inactive";
    floor: number;
    location: string;
    equipment: string[];
}

export interface FilterRoom {
    capacity?: number;
    equipment?: Equipment[];
    floor?: number
}