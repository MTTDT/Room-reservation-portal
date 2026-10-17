export const ReservationStatus = {
  Booked: 0,
  Completed: 1,
  Cancelled: 2,
} as const;

export type ReservationStatus = (typeof ReservationStatus)[keyof typeof ReservationStatus];

export const reservationStatusLabels: Record<ReservationStatus, string> = {
  [ReservationStatus.Booked]: "Booked",
  [ReservationStatus.Completed]: "Completed",
  [ReservationStatus.Cancelled]: "Cancelled",
};