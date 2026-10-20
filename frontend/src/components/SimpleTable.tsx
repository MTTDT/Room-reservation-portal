
import {
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
} from "@mui/material";

export interface SimpleTableProps<T>{
    headers: string[];
    keys: (keyof T)[];
    objs: T[];
}

export default function SimpleTable <T extends object>({ headers, keys, objs }:  SimpleTableProps<T> ){
 return(
    <TableContainer component={Paper}>
        <Table>
            <TableHead>
                <TableRow>
                    {headers.map((t, i) => <TableCell key={i}>{t}</TableCell>)}
                </TableRow>
            </TableHead>
            <TableBody>
                {objs.map((o, i) => (
                    <TableRow key={i}>
                        {keys.map((k) => 
                            <TableCell key={String(k)}>
                                {Array.isArray(o[k]) ? 
                                    o[k].map((item: string) => (
                                        <span key={item}>{String(item)} </span>
                                    )) : String(o[k])
                                }
                            </TableCell>)}
                    </TableRow>
                ))}
            </TableBody>
        </Table>
    </TableContainer>
 )
}