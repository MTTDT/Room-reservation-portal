import { reservationStatusLabels } from '../types/reservationStatus'

import { useQuery } from '@tanstack/react-query'
import {
  Alert, CircularProgress, Paper, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, Typography,
} from '@mui/material'
import { getReservations } from '../api/reservationApi'

export default function RoomsPage() {
  const { data: reservations, isLoading, error } = useQuery({
    queryKey: ['reservations'],
    queryFn: getReservations,
  })

  if (isLoading) return <CircularProgress />
  if (error) return <Alert severity="error">{error.message}</Alert>

  return (
    <>
        <Typography variant="h4" gutterBottom>Rooms</Typography>
            <TableContainer component={Paper}>
                <Table>
                <TableHead>
                    <TableRow>
                    <TableCell>ID</TableCell>
                    <TableCell>Title</TableCell>
                    <TableCell>Booked by</TableCell>
                    <TableCell>Room name</TableCell>
                    <TableCell>Strating time</TableCell>
                    <TableCell>Ending time</TableCell>
                    <TableCell>Number of attendees</TableCell>
                    <TableCell>Status</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {reservations?.map((reservation) => (
                    <TableRow key={reservation.id}>
                        <TableCell>{reservation.id}</TableCell>
                        <TableCell>{reservation.title}</TableCell>
                        <TableCell>{reservation.userName} {reservation.userSurname}</TableCell>
                        <TableCell>{reservation.roomName}</TableCell>
                        <TableCell>{reservation.startTime}</TableCell>
                        <TableCell>{reservation.endTime}</TableCell>
                        <TableCell>{reservation.attendeeCount}</TableCell>
                        <TableCell>{reservationStatusLabels[reservation.status]}</TableCell>

                    </TableRow>
                    ))}
                </TableBody>
                </Table>
            </TableContainer>
    </>
  )
}