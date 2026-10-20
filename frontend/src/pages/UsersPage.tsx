import { useQuery } from "@tanstack/react-query";
import {
    Alert,
    CircularProgress,
    Typography,
} from "@mui/material";
import { getUsers } from "../api/client/userApi";
import SimpleTable from "../components/SimpleTable";
import type { User } from "../types/user";

export default function UsersPage() {
    const {
        data: users,
        isLoading,
        error,
    } = useQuery({
        queryKey: ["users"],
        queryFn: getUsers,
    });

    if (isLoading) return <CircularProgress />;
    if (error) return <Alert severity="error">{error.message}</Alert>;

    const tableHeaders: string[] = [
        "ID",
        "Name",
        "Surname",
        "Active Registrations",
    ];

        const tableKeys: (keyof User)[] = [
        "id",
        "name",
        "surname",
        "activeRegistrations",
    ];

    return (
        <>
            <Typography variant="h4" gutterBottom>
                Users
            </Typography>
            <SimpleTable<User> headers={tableHeaders} keys={tableKeys} objs={users as User[]}/>

        </>
    );
}
