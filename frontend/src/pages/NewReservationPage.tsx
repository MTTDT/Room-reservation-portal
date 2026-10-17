import * as React from 'react';
import { useState } from 'react';
import { RoomTimeline, type Slot} from '../components/claude';
import type { Room } from '../types/room'
import type { Reservation } from '../types/reservation';
import { Box, Alert, CircularProgress,} from "@mui/material"
import ReservationForm from '../components/ReservationForm';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getReservations } from '../api/reservationApi';
import { getRooms } from '../api/roomApi';
import { getStoredUserId } from '../utils/userStoring';
import type { CreateReservation } from '../types/reservation';
import { createReservation } from "../api/reservationApi";




export default function NewReservationPage() {
    const queryClient = useQueryClient();
    const [slot, setSlot] = useState<Slot | null>(null);
    const userId: number | null = getStoredUserId()

    const { data: rooms, isLoading: roomsLoading, error: roomsError } = useQuery({
        queryKey: ["rooms"],
        queryFn: getRooms,
    });

    const {data: reservations, isLoading: reservationsLoading, error: reservationsError,} = useQuery({
        queryKey: ["reservations"],
        queryFn: getReservations, 
    });

    const mutation = useMutation({
        mutationFn: createReservation,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["reservations"] });
            setSlot(null);
        },
    });


    if (roomsLoading || reservationsLoading) return <CircularProgress />;
    if (roomsError || reservationsError || !rooms || !reservations) {
        return <Alert severity="error">Failed to load data</Alert>;
    }



    const handleSlotClicked = ({ roomId, start }: {roomId: number, start: Date}) => {
        if (userId === null) return;
       setSlot({roomId, start})
    }

    const handleSubmit = (value: CreateReservation) =>{
                console.log("creating", value)

        mutation.mutate(value);
    }



    return (
        <Box>
            <RoomTimeline
                rooms={rooms}
                reservations={reservations}
                defaultView="month"
                userId={userId}
                onSlotClick={({ roomId, start }) => {
                    handleSlotClicked({ roomId, start })
                }}
                // onBookingClick={(b) => {
                //     // open your own edit dialog here
                //     if (window.confirm(`Delete booking for ${b.guest}?`)) {
                //     setBookings((prev) => prev.filter((x) => x.id !== b.id));
                //     }
                // }}
            />
            {slot && userId !== null && (
                <ReservationForm 
                    open
                    userId={userId}
                    roomId={slot.roomId}
                    startTime={String(slot.start)}
                    handleSubmit={handleSubmit}
                    handleClose={() => setSlot(null)}
                    error={mutation.error?.message}
                    isSubmitting={mutation.isPending}
                />



            )}



                
            
            
        </Box>
    );        
}