import { useState } from 'react';
import { RoomTimeline, type Slot} from '../components/claude';
import { Box, Alert, CircularProgress, FormControlLabel, Switch} from "@mui/material"
import ReservationForm from '../components/ReservationForm';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getReservations } from '../api/reservationApi';
import { getRooms } from '../api/roomApi';
import { getStoredUserId } from '../utils/userStoring';
import type { CreateReservation, UpdateReservation } from '../types/reservation';
import { createReservation, updateReservation } from "../api/reservationApi";
import { ReservationStatus } from '../types/reservationStatus';





export default function NewReservationPage() {
    const queryClient = useQueryClient();
    const [slot, setSlot] = useState<Slot | null>(null);
    const [updateRes, setUpdateRes] = useState<UpdateReservation | null>(null);
    const [resId, setResId] = useState<number | null>(null);
    const [showOnlyMy, setShowOnlyMy] = useState<boolean>(false);
    const userId: number | null = getStoredUserId();

    const { data: rooms, isLoading: roomsLoading, error: roomsError } = useQuery({
        queryKey: ["rooms"],
        queryFn: getRooms,
    });

    const {data: reservations, isLoading: reservationsLoading, error: reservationsError,} = useQuery({
        queryKey: ["reservations"],
        queryFn: getReservations,  
    });

    const createMutation = useMutation({
        mutationFn: createReservation,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["reservations"] });
            setSlot(null);
        },
    });
    
    const updateMutation = useMutation({
        mutationFn: ({ data, id }: {  data: UpdateReservation; id: number }) => updateReservation(data, id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["reservations"] });
            setUpdateRes(null);
        },
    })


    if (roomsLoading || reservationsLoading) return <CircularProgress />;
    if (roomsError || reservationsError || !rooms || !reservations) {
        return <Alert severity="error">Failed to load data</Alert>;
    }



    const handleSlotClicked = ({ roomId, start }: {roomId: number, start: Date}) => {
        if (userId === null) return;
        setSlot({roomId, start})
    }

    const handleBookingClicked = ({ updateReq, resId,  uId }: { updateReq: UpdateReservation, resId: number, uId: number}) => {
        if (userId === null) return;
        if (userId !== uId) return;
        if (updateReq.status != ReservationStatus.Booked) return null;
        setResId(resId);
        setUpdateRes(updateReq);
    }

    const handleSubmit = (value: CreateReservation | UpdateReservation) =>{
        if(resId !== null) updateMutation.mutate({data: value as UpdateReservation, id: resId});
        else createMutation.mutate(value as CreateReservation);
    }

    const handleFormClose = () => {
        setSlot(null);
        setUpdateRes(null);
        setResId(null);
        updateMutation.reset;
        createMutation.reset;
    }

    return (
        <Box>
            <FormControlLabel label="Show only my" control={<Switch checked={showOnlyMy} onChange={(e) => {setShowOnlyMy(e.target.checked)}}/>}/>
            <RoomTimeline
                showOnlyMy={showOnlyMy}
                rooms={rooms}
                reservations={reservations}
                defaultView="month"
                userId={userId}
                onSlotClick={({ roomId, start }) => {
                    handleSlotClicked({ roomId, start })
                }}
                onBookingClick={(updateReq, resId, uId) => {
                    handleBookingClicked({updateReq, resId, uId})
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
    );        
}