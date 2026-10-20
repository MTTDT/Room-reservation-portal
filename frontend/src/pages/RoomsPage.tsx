import { keepPreviousData, useQuery } from "@tanstack/react-query";
import {
    Alert,
    CircularProgress,
    Typography,
} from "@mui/material";
import { getRooms } from "../api/client/roomApi";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useEffect, useMemo } from "react";
import { buildQuery, parseQuery } from "../utils/query";
import type { FilterRoom, Room } from "../types/room";
import RoomFilter from "../components/RoomFilter";
import FilterPanel from "../components/FilterPanel";
import SimpleTable from "../components/SimpleTable";

export default function RoomsPage() {

    const navigate = useNavigate();

    const [searchParams, setSearchParams] = useSearchParams();

  
    const filter = useMemo(() => parseQuery(searchParams.toString()), [searchParams]);

    const tableHeaders: string[] = [
        "ID",
        "Name",
        "Capacity",
        "Status",
        "Floor",
        "Location",
        "Equipment",
    ];

    const tableKeys: (keyof Room)[] = [
        "id",
        "name",
        "capacity",
        "status",
        "floor",
        "location",
        "equipment",
    ];

    useEffect(() => {
        navigate({search : buildQuery(filter)}, {replace: true});
    }, [filter, navigate])

    const { data: rooms, isLoading, error, } = useQuery({
        queryKey: ["rooms", filter],
        queryFn: getRooms,
        placeholderData: keepPreviousData,
    });

    if (isLoading) return <CircularProgress />;
    if (error) return <Alert severity="error">{error.message}</Alert>;

    const handleFilterChange = <K extends keyof FilterRoom>(key: K, value: FilterRoom[K]) => {
        setSearchParams(buildQuery({ ...filter, [key]: value }), { replace: true });
    }

    const handleResetFilter = () => {
      setSearchParams({}, { replace: true })

    }

    return (
        <>
            <Typography variant="h4" gutterBottom>
                Rooms
            </Typography>
             <FilterPanel onReset={handleResetFilter}>
                  <RoomFilter filters={filter} handleChange={handleFilterChange}/>
              </FilterPanel>
              <SimpleTable<Room> headers={tableHeaders} keys={tableKeys} objs={rooms as Room[]}/>
        </>
    );
}
