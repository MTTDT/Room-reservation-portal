import { useState, type ChangeEvent, type SubmitEvent } from "react";
import { Box, TextField, Alert, Dialog, DialogTitle, Button } from "@mui/material";
import type { CreateReservation, UpdateReservation } from "../types/reservation";
import { type ReservationStatus, ReservationStatusValue } from "../types/reservationStatus";
import dayjs, { Dayjs } from "dayjs";
import { DateTimePicker } from "@mui/x-date-pickers/DateTimePicker";

export interface ReservationFormProps {
    updateRes: UpdateReservation | null;
    open: boolean;
    userId: number;
    roomId?: number;
    startTime?: Date;
    reservation?: UpdateReservation;
    handleSubmit: (value: CreateReservation | UpdateReservation) => void;
    handleClose: () => void;
    error?: string;
    isSubmitting: boolean;
}

interface FormValues {
    title: string;
    from: Dayjs | null;
    to: Dayjs | null;
    attendeeCount: number;
    status?: ReservationStatus;
}

export default function ReservationForm(props: ReservationFormProps) {
    const [values, setValues] = useState<FormValues>({
        title: props.updateRes?.title ?? "",
        from: props.updateRes ? dayjs(props.updateRes.startTime) : dayjs(props.startTime),
        to: props.updateRes
            ? dayjs(props.updateRes.endTime)
            : dayjs(props.startTime).add(1, "hour"),
        attendeeCount: props.updateRes?.attendeeCount ?? 0,
        status: props.updateRes?.status,
    });

    const handleChange = (e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        setValues((prev) => ({ ...prev, [name]: value }));
    };

    const currentlyHappening = () => {
        const now = dayjs();
        const { from, to } = values;
        if (!from || !to) return false;
        return !from.isAfter(now) && !to.add(1, "hour").isBefore(now);
    };
    const willHappen = () => {
        const now = dayjs();
        if (!values.from) return false;
        return values.from.isAfter(now);
    };

    const handleSubmit = (e: SubmitEvent<HTMLFormElement>) => {
        e.preventDefault();

        if (props.updateRes !== null) {
            const reservation: UpdateReservation = {
                title: values.title,
                startTime: values.from!.toISOString(),
                endTime: values.to!.toISOString(),
                attendeeCount: Number(values.attendeeCount),
                status: values.status ?? ReservationStatusValue.Booked,
            };
            props.handleSubmit(reservation);
        } else {
            const reservation: CreateReservation = {
                roomId: props.roomId ?? 0,
                userId: props.userId,
                title: values.title,
                startTime: values.from!.toISOString(),
                endTime: values.to!.toISOString(),
                attendeeCount: Number(values.attendeeCount),
            };
            props.handleSubmit(reservation);
        }
    };

    const handleCancel = () => {
        props.handleSubmit({ status: ReservationStatusValue.Cancelled } as UpdateReservation);
    };

    const handleComplete = () => {
        props.handleSubmit({ status: ReservationStatusValue.Completed } as UpdateReservation);
    };

    return (
        <Dialog onClose={props.handleClose} open={props.open}>
            <DialogTitle sx={{ mx: "auto" }}>Create new reservation</DialogTitle>
            <Box
                component="form"
                onSubmit={handleSubmit}
                noValidate
                sx={{
                    mx: "auto",
                    m: 4,
                    display: "flex",
                    flexDirection: "column",
                    gap: 2,
                }}
            >
                <TextField
                    type="text"
                    onChange={handleChange}
                    name="title"
                    value={values.title}
                    required
                />
                <DateTimePicker
                    label="From"
                    value={values.from}
                    onChange={(v) => setValues((prev) => ({ ...prev, from: v }))}
                    ampm={false}
                    minutesStep={15}
                    slotProps={{
                        actionBar: { actions: ["cancel", "accept"] },
                        textField: { required: true },
                    }}
                />
                <DateTimePicker
                    label="To"
                    value={values.to}
                    onChange={(v) => setValues((prev) => ({ ...prev, to: v }))}
                    minDateTime={values.from ?? undefined}
                    ampm={false}
                    minutesStep={15}
                    slotProps={{
                        actionBar: { actions: ["cancel", "accept"] },
                        textField: { required: true },
                    }}
                />
                <TextField
                    type="number"
                    onChange={handleChange}
                    name="attendeeCount"
                    value={values.attendeeCount}
                    required
                />

                <Button
                    type="submit"
                    variant="contained"
                    disabled={props.isSubmitting}
                    loading={props.isSubmitting}
                >
                    {props.updateRes ? "Update reservation" : "Submit reservation"}
                </Button>

                {props.updateRes && currentlyHappening() && (
                    <Button type="button" variant="contained" onClick={handleComplete}>
                        Complete meeting early
                    </Button>
                )}

                {props.updateRes && willHappen() && (
                    <Button type="button" variant="contained" onClick={handleCancel}>
                        Cancel reservation
                    </Button>
                )}

                {props.error && <Alert severity="error">{props.error}</Alert>}
            </Box>
        </Dialog>
    );
}
