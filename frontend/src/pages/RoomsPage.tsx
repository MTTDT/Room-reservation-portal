import { useQuery } from '@tanstack/react-query'
import {
  Alert, CircularProgress, Paper, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, Typography,
} from '@mui/material'
import { getRooms } from '../api/roomApi'

export default function RoomsPage() {
  const { data: rooms, isLoading, error } = useQuery({
    queryKey: ['rooms'],
    queryFn: getRooms,
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
              <TableCell>Name</TableCell>
              <TableCell>Capacity</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Floor</TableCell>
                <TableCell>Location</TableCell>
                <TableCell>Equipment</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {rooms?.map((room) => (
              <TableRow key={room.id}>
                <TableCell>{room.id}</TableCell>
                <TableCell>{room.name}</TableCell>
                <TableCell>{room.capacity}</TableCell>
                <TableCell>{room.status}</TableCell>
                <TableCell>{room.floor}</TableCell>
                <TableCell>{room.location}</TableCell>
                <TableCell>
                  {room.equipment.map((item: string) => (
                    <span key={item}>{item}</span>
                  ))}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </>
  )
}