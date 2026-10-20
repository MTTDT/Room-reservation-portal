import { Box, Button } from "@mui/material"
import { useState, type ReactNode } from "react"

interface FilterPanelProps {
  children: ReactNode;
  onReset?: () => void;
}

export default function FilterPanel({ children, onReset }: FilterPanelProps){
    const [isOpen, setIsOpen] = useState(false);

    return (
        <Box>
            <Button size="small" onClick={() => setIsOpen(o => !o)}>
                {isOpen ? "Hide Filters" : "Show Filters" }
            </Button>
            {isOpen && (
                <Box sx={{display: "flex", flexWrap: "wrap", gap: 2, mt: 1, mb: 2, alignItems: "center" }}>
                    {children}
                    <Button onClick={onReset}>Reset</Button>
                </Box>
            )}
        </Box>
    )
}