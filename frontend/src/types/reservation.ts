import type { ReservationStatus } from "./reservationStatus";
export interface Reservation {
    id: number;
    title: string;
    userId: number;
    userName: string;
    userSurname: string;
    roomId: number;
    roomName: string;
    startTime: string;
    endTime: string;
    attendeeCount: number;
    status: ReservationStatus;
}

export interface CreateReservation {
    roomId: number;
    userId: number;
    title: string;
    startTime: string;
    endTime: string;
    attendeeCount: number;
}

export interface UpdateReservation {
    title?: string;
    startTime?: string;
    endTime?: string;
    attendeeCount?: number;
    status?: ReservationStatus;
}

export interface FilterReservation {
    roomName?: string;
    organizer?: string;
    status?: ReservationStatus;
    from?: string;
    to?: string;
}
