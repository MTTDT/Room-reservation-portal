import {
    Box,
    TextField,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    type SelectChangeEvent,
    OutlinedInput,
} from "@mui/material";
import type { FilterRoom } from "../types/room";
import { EQUIPMENT, type Equipment } from "../types/equipment";

interface RoomFilterProps {
    filters: FilterRoom;
    handleChange: <K extends keyof FilterRoom>(key: K, value: FilterRoom[K]) => void;
}

export default function RoomFilter({ filters, handleChange }: RoomFilterProps) {
    const handleSelectedEquipmentChange = (event: SelectChangeEvent<Equipment[]>) => {
        const {
            target: { value },
        } = event;
        const newSelected = typeof value === "string" ? (value.split(",") as Equipment[]) : value;
        handleChange("equipment", newSelected);
    };

    const equipment = ([] as Equipment[]).concat(filters.equipment ?? []);
    return (
        <Box>
            <Box sx={{ display: "flex", flexWrap: "wrap", gap: 2 }}>
                <TextField
                    type="number"
                    label="Min capacity"
                    variant="outlined"
                    value={filters.capacity ?? ""}
                    onChange={(e) => handleChange("capacity", Number(e.target.value))}
                />
                <TextField
                    type="number"
                    label="Floor"
                    variant="outlined"
                    value={filters.floor ?? ""}
                    onChange={(e) => handleChange("floor", Number(e.target.value))}
                />
                <FormControl sx={{ width: 300 }}>
                    <InputLabel id="select-equipment">Select Equipment</InputLabel>
                    <Select
                        labelId="select-equipment"
                        id="equipment-selector"
                        multiple
                        value={equipment}
                        onChange={handleSelectedEquipmentChange}
                        input={<OutlinedInput label="Select Equipment" />}
                    >
                        {EQUIPMENT.map((eq) => (
                            <MenuItem key={eq} value={eq}>
                                {eq}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>
            </Box>
        </Box>
    );
}
