export const RESERVATION_STATUS = ["Booked", "Completed", "Cancelled"] as const;

export type ReservationStatus = (typeof RESERVATION_STATUS)[number];

export const RESERVATION_STATUS_LABELS: Record<ReservationStatus, string> = {
  Booked: "Booked",
  Completed: "Completed",
  Cancelled: "Cancelled",
};

export const ReservationStatusValue = {
  Booked: "Booked",
  Completed: "Completed",
  Cancelled: "Cancelled",
} as const;