import { useState, type ChangeEvent, type SubmitEvent } from "react";
import { Box, TextField, Alert, CircularProgress, Dialog, DialogTitle, Button } from "@mui/material";
import type { CreateReservation } from "../types/reservation";
import dayjs, { type Dayjs } from "dayjs";
import { DateTimePicker } from "@mui/x-date-pickers/DateTimePicker";


export interface ReservationFormProps{
    open: boolean,
    userId: number,
    roomId: number,
    startTime: Date,
    handleSubmit: (value: CreateReservation) => void,
    handleClose: () => void,
    error?: string,
    isSubmitting: boolean
}



export default function ReservationForm(props: ReservationFormProps){
    const [values, setValues] = useState<{
        title: string;
        from: Dayjs | null;
        to: Dayjs | null;
        attendeeCount: string;
    }>({
        title: "",
        from: dayjs(props.startTime),
        to: dayjs(props.startTime).add(1, "hour"),
        attendeeCount: "",
    });
    
    const [errors, setErrors] = useState({});

    
    const handleChange = (e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        setValues((prev) => ({ ...prev, [name]: value}))
    }

    const validate = () => {};

    const handleSubmit = (e: SubmitEvent<HTMLFormElement>) => {
        e.preventDefault();
        const reservation: CreateReservation = 
        {
            roomId: props.roomId,
            userId: props.userId,
            title: values.title,
            startTime: values.from!.toISOString(),
            endTime: values.to!.toISOString(),
            atendeeCount: Number(values.attendeeCount)
        }
        props.handleSubmit(reservation)
    }

    return(
        <Dialog onClose={props.handleClose} open={props.open}>
            <DialogTitle sx={{mx: "auto"}}>Create new reservation</DialogTitle>
            <Box 
                component="form"
                onSubmit={handleSubmit}
                noValidate
                sx={{
                    mx: "auto",
                    m:4,
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
                <Button type="submit" variant="contained" disabled={props.isSubmitting}>
                    {props.isSubmitting ? (
                        <CircularProgress />
                    ):  <p>Submit reservation</p>}
                   
                </Button>
                {props.isSubmitting && (
                    <CircularProgress />
                )}
                {props.error && <Alert severity="error">{props.error}</Alert>}
            </Box>
        </Dialog>
    )
}