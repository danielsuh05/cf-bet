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
import { getRankings } from "../services/contestService";

const columns = [
  {
    key: "username",
    label: "USERNAME",
  },
  {
    key: "pl",
    label: "TOTAL PROFIT/LOSS",
  },
];

export default function RankingsTable() {
  const [users, setUsers] = useState([]);

  useEffect(() => {
    async function fetchUserRankings() {
      try {
        const response = await getRankings();
        setUsers(response);
      } catch (error) {
        console.error("Error fetching contests:", error);
      }
    }
    fetchUserRankings();
  }, []);

  return (
    <>
      <Table selectionMode="single" isStriped color="default">
        <TableHeader columns={columns}>
          {(column) => (
            <TableColumn key={column.key}>{column.label}</TableColumn>
          )}
        </TableHeader>
        <TableBody items={users}>
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
