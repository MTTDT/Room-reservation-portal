import { DateTimePicker } from "@mui/x-date-pickers/DateTimePicker";
import type { FilterReservation } from "../types/reservation";
import { type ReservationStatus, RESERVATION_STATUS_LABELS } from "../types/reservationStatus";

import {
    Box,
    TextField,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    type SelectChangeEvent,
    OutlinedInput,
} from "@mui/material";
import dayjs from "dayjs";

interface ReservationFilterProps {
    filters: FilterReservation;
    handleChange: <K extends keyof FilterReservation>(key: K, value: FilterReservation[K]) => void;
}

export default function ReservationFilter({ filters, handleChange }: ReservationFilterProps) {
    const handleStatusChange = (e: SelectChangeEvent<ReservationStatus | "">) => {
        const v = e.target.value;
        handleChange("status", v === "" ? undefined : (v as ReservationStatus));
    };

    return (
        <Box>
            <Box sx={{ display: "flex", flexWrap: "wrap", gap: 2 }}>
                <TextField
                    label="Room name"
                    variant="outlined"
                    value={filters.roomName ?? ""}
                    onChange={(e) => handleChange("roomName", e.target.value)}
                />
                <TextField
                    label="Organizer"
                    variant="outlined"
                    value={filters.organizer ?? ""}
                    onChange={(e) => handleChange("organizer", e.target.value)}
                />
                <FormControl sx={{ width: 100 }}>
                    <InputLabel id="change-status">Status</InputLabel>
                    <Select
                        labelId="change-status"
                        id="status-select"
                        value={filters.status ?? ""}
                        onChange={handleStatusChange}
                        input={<OutlinedInput label="Status" />}
                    >
                        <MenuItem value="">Any</MenuItem>
                        {Object.entries(RESERVATION_STATUS_LABELS).map(([value, label]) => (
                            <MenuItem key={value} value={value}>
                                {label}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>
                <DateTimePicker
                    label="From"
                    value={filters.from ? dayjs(filters.from) : null}
                    onChange={(e) => handleChange("from", e?.toISOString().slice(0, 16))}
                    ampm={false}
                    minutesStep={15}
                    slotProps={{
                        actionBar: { actions: ["cancel", "accept"] },
                        textField: { required: false },
                    }}
                />
                <DateTimePicker
                    label="To"
                    value={filters.to ? dayjs(filters.to) : null}
                    onChange={(e) => handleChange("to", e?.toISOString().slice(0, 16))}
                    ampm={false}
                    minutesStep={15}
                    slotProps={{
                        actionBar: { actions: ["cancel", "accept"] },
                        textField: { required: false },
                    }}
                />
            </Box>
        </Box>
    );
}
