import { useEffect, useMemo, useState } from "react";
import { useQuery, useMutation, useQueryClient, keepPreviousData } from "@tanstack/react-query";
import { Alert, CircularProgress, Box, Switch, FormControlLabel } from "@mui/material";
import ReservationForm from "../components/ReservationForm";
import { RoomTimeline, type Slot } from "../components/RoomTimeline";
import type {
    UpdateReservation,
    CreateReservation,
    FilterReservation,
    Reservation,
} from "../types/reservation";
import { ReservationStatusValue } from "../types/reservationStatus";
import {
    getReservations,
    createReservation,
    updateReservation,
} from "../api/client/reservationApi";
import { getRooms } from "../api/client/roomApi";
import { getStoredUserId } from "../utils/userStoring";
import ReservationFilter from "../components/ReservationFilter";
import { buildQuery, parseQuery } from "../utils/query";
import { useNavigate, useSearchParams } from "react-router-dom";
import FilterPanel from "../components/FilterPanel";
import SimpleTable from "../components/SimpleTable";

export default function RoomsPage() {
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const [searchParams, setSearchParams] = useSearchParams();

    const [slot, setSlot] = useState<Slot | null>(null);
    const [updateRes, setUpdateRes] = useState<UpdateReservation | null>(null);
    const [resId, setResId] = useState<number | null>(null);
    const [showOnlyMy, setShowOnlyMy] = useState<boolean>(false);
    const filter = useMemo(() => parseQuery(searchParams.toString()), [searchParams]);

    const userId: number | null = getStoredUserId();

    const tableHeaders: string[] = [
        "ID",
        "Title",
        "Booked by",
        "Room name",
        "Starting time",
        "Ending time",
        "Number of attendees",
        "Status",
    ];
    const tableKeys: (keyof Reservation)[] = [
        "id",
        "title",
        "userName",
        "roomName",
        "startTime",
        "endTime",
        "attendeeCount",
        "status",
    ];

    useEffect(() => {
        navigate({ search: buildQuery(filter) });
    }, [filter, navigate]);

    const {
        data: rooms,
        isLoading: roomsLoading,
        error: roomsError,
    } = useQuery({
        queryKey: ["rooms"],
        queryFn: getRooms,
    });

    const {
        data: reservations,
        isLoading: reservationsLoading,
        error: reservationsError,
    } = useQuery({
        queryKey: ["reservations", filter],
        queryFn: getReservations,
        placeholderData: keepPreviousData,
    });

    const createMutation = useMutation({
        mutationFn: createReservation,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["reservations"] });
            setSlot(null);
            setResId(null);
        },
    });

    const updateMutation = useMutation({
        mutationFn: ({ data, id }: { data: UpdateReservation; id: number }) =>
            updateReservation(data, id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["reservations"] });
            setUpdateRes(null);
            setResId(null);
        },
    });

    if (roomsLoading || reservationsLoading) return <CircularProgress />;
    if (roomsError || reservationsError || !rooms || !reservations) {
        return <Alert severity="error">Failed to load data</Alert>;
    }

    const handleSlotClicked = ({ roomId, start }: { roomId: number; start: Date }) => {
        if (userId === null) return;
        setSlot({ roomId, start });
    };

    const handleBookingClicked = ({
        updateReq,
        resId,
        uId,
    }: {
        updateReq: UpdateReservation;
        resId: number;
        uId: number;
    }) => {
        if (userId === null) return;
        if (userId !== uId) return;
        if (updateReq.status != ReservationStatusValue.Booked) return null;
        setResId(resId);
        setUpdateRes(updateReq);
    };

    const handleSubmit = (value: CreateReservation | UpdateReservation) => {
        if (resId !== null) updateMutation.mutate({ data: value as UpdateReservation, id: resId });
        else createMutation.mutate(value as CreateReservation);
    };

    const handleFormClose = () => {
        setSlot(null);
        setUpdateRes(null);
        setResId(null);
        createMutation.reset();
        updateMutation.reset();
    };

    const handleFilterChange = <K extends keyof FilterReservation>(
        key: K,
        value: FilterReservation[K]
    ) => {
        setSearchParams(buildQuery({ ...filter, [key]: value }), { replace: true });
    };

    const handleResetFilter = () => {
        setSearchParams({}, { replace: true });
    };

    return (
        <Box>
            <Box>
                <FormControlLabel
                    label="Show only my"
                    control={
                        <Switch
                            checked={showOnlyMy}
                            onChange={(e) => {
                                setShowOnlyMy(e.target.checked);
                            }}
                        />
                    }
                />
                <FilterPanel onReset={handleResetFilter}>
                    <ReservationFilter filters={filter} handleChange={handleFilterChange} />
                </FilterPanel>
            </Box>
            <Box>
                <RoomTimeline
                    showOnlyMy={showOnlyMy}
                    rooms={rooms}
                    reservations={reservations}
                    defaultView="month"
                    userId={userId}
                    onSlotClick={({ roomId, start }) => {
                        handleSlotClicked({ roomId, start });
                    }}
                    onBookingClick={(updateReq, resId, uId) => {
                        handleBookingClicked({ updateReq, resId, uId });
                    }}
                />
                {(updateRes || slot) && userId !== null && (
                    <ReservationForm
                        updateRes={updateRes}
                        open
                        userId={userId}
                        roomId={slot?.roomId}
                        startTime={slot?.start}
                        handleSubmit={handleSubmit}
                        handleClose={handleFormClose}
                        error={updateMutation.error?.message || createMutation.error?.message}
                        isSubmitting={updateMutation.isPending || createMutation.isPending}
                    />
                )}
            </Box>
            <SimpleTable<Reservation>
                headers={tableHeaders}
                keys={tableKeys}
                objs={reservations as Reservation[]}
            />
        </Box>
    );
}
