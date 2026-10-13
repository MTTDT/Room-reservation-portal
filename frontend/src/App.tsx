import { Link, Navigate, Route, Routes } from 'react-router-dom'
import { AppBar, Button, Container, Toolbar, Typography } from '@mui/material'
import UsersPage from './pages/UsersPage'
import RoomsPage from './pages/RoomsPage'

export default function App() {
  return (
    <>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>
            Room Reservation
          </Typography>
          <Button color="inherit" component={Link} to="/users">Users</Button>
          <Button color="inherit" component={Link} to="/rooms">Rooms</Button>
        </Toolbar>
      </AppBar>

      <Container sx={{ mt: 4 }}>
        <Routes>
          <Route path="/users" element={<UsersPage />} />
          <Route path="/rooms" element={<RoomsPage />} />
          {/* <Route path="*" element={<Navigate to="/users" replace />} /> */}
        </Routes>
      </Container>
    </>
  )
}