import { reservationStatusLabels } from '../types/reservationStatus'

import { Paper, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow
} from '@mui/material'
import type { Reservation } from '../types/reservation'


export default function ReservationTable({ reservations }: { reservations: Reservation[] } ) {
  
    return (
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
  )
}