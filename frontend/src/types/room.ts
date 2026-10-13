export interface Room {
    id: number;
    name: string;
    capacity: number;
    status: 'Active' | 'Inactive';
    floor: number;
    location: string;
    equipment: string[];
}