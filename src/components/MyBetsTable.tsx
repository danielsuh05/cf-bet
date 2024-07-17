import { useState, useEffect } from "react";
import {
  Table,
  TableHeader,
  TableBody,
  TableColumn,
  TableRow,
  TableCell,
} from "@nextui-org/table";
import { getKeyValue } from "@nextui-org/react";
import { getUserBets } from "../services/userService";

const columns = [
  {
    key: "details",
    label: "DETAILS",
  },
  {
    key: "contest",
    label: "CONTEST",
  },
  {
    key: "date",
    label: "DATE",
  },
  {
    key: "profitloss",
    label: "PROFIT/LOSS",
  },
];

export default function MyBetsTable() {
  const [bets, setBets] = useState([]);

  useEffect(() => {
    async function fetchMyBets() {
      try {
        const response = await getUserBets("danielsuh", "jwt");
        setBets(response);
      } catch (error) {
        console.error("Error fetching contests:", error);
      }
    }
    fetchMyBets();
  }, []);

  return (
    <>
      <Table selectionMode="single" isStriped color="default">
        <TableHeader columns={columns}>
          {(column) => (
            <TableColumn key={column.key}>{column.label}</TableColumn>
          )}
        </TableHeader>
        <TableBody items={bets}>
          {(item: any) => (
            <TableRow key={item.relativeTimeSeconds}>
              {(columnKey) => (
                <TableCell>{getKeyValue(item, columnKey)}</TableCell>
              )}
            </TableRow>
          )}
        </TableBody>
      </Table>
    </>
  );
}
