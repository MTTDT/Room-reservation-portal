import { useQuery } from "@tanstack/react-query";

import { useState } from "react";
import { Link, Route, Routes } from "react-router-dom";
import {
    AppBar,
    Button,
    Container,
    Toolbar,
    Typography,
    FormControl,
    InputLabel,
    MenuItem,
    Select,
    Box,
    type SelectChangeEvent,
    Alert,
    CircularProgress,
} from "@mui/material";
import UsersPage from "./pages/UsersPage";
import RoomsPage from "./pages/RoomsPage";
import NotFoundPage from "./pages/NotFoundPage";
import ReservationsPage from "./pages/ReservationsPage";
import WelcomePage from "./pages/WelcomePage";
import { getUsers } from "./api/client/userApi";
import { getStoredUserId, storeUserId } from "./utils/userStoring";

export default function App() {
    const {
        data: users,
        isLoading,
        error,
    } = useQuery({
        queryKey: ["users"],
        queryFn: getUsers,
    });

    const [userId, setUserId] = useState<number | null>(() => getStoredUserId());

    if (isLoading) return <CircularProgress />;
    if (error) return <Alert severity="error">{error.message}</Alert>;

    const currentUser = users?.find((u) => u.id === userId);

    const handleChangeUser = (e: SelectChangeEvent<number | string>) => {
        const id = Number(e.target.value);
        setUserId(id);
        storeUserId(id);
    };
    return (
        <>
            <AppBar position="static">
                <Toolbar>
                    <Typography variant="h6" sx={{ flexGrow: 1 }}>
                        Room Reservation
                    </Typography>
                    <Box color="inherit">
                        <FormControl fullWidth>
                            <InputLabel id="change-user"></InputLabel>
                            <Select
                                labelId="change-user"
                                id="user-select"
                                value={currentUser?.id ?? "Select user"}
                                onChange={handleChangeUser}
                            >
                                {users?.map((user) => (
                                    <MenuItem key={user.id} value={user.id}>
                                        {user.name} {user.surname}
                                    </MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                    </Box>
                    <Button color="inherit" component={Link} to="/users">
                        Users
                    </Button>
                    <Button color="inherit" component={Link} to="/rooms">
                        Rooms
                    </Button>
                    <Button color="inherit" component={Link} to="/reservations">
                        Reservations
                    </Button>
                </Toolbar>
            </AppBar>

            <Container sx={{ mt: 4 }}>
                <Routes>
                    <Route path="/users" element={<UsersPage />} />
                    <Route path="/rooms" element={<RoomsPage />} />
                    <Route path="/reservations" element={<ReservationsPage />} />
                    <Route path="/" element={<WelcomePage />} />

                    <Route path="*" element={<NotFoundPage />} />
                </Routes>
            </Container>
        </>
    );
}
