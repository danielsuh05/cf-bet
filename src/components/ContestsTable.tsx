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
import { getContests } from "../services/contestService";
import { secondsToDate } from "../utils/utils";

const columns = [
  {
    key: "name",
    label: "CONTEST",
  },
  {
    key: "startTimeSeconds",
    label: "START",
  },
  {
    key: "link",
    label: "BET",
  },
];

export default function ContestsTable() {
  const [contests, setContests] = useState([]);

  useEffect(() => {
    async function fetchContests() {
      try {
        const response = await getContests();
        setContests(response);
      } catch (error) {
        console.error("Error fetching contests:", error);
      }
    }
    fetchContests();
  }, []);

  return (
    <>
      <Table selectionMode="single" isStriped color="default">
        <TableHeader columns={columns}>
          {(column) => (
            <TableColumn className="min-w-[190px]" key={column.key}>
              {column.label}
            </TableColumn>
          )}
        </TableHeader>
        <TableBody items={contests} emptyContent="No active contests.">
          {(item: any) => (
            <TableRow key={item.relativeTimeSeconds}>
              {(columnKey) => (
                <TableCell>
                  {columnKey === "link" ? (
                    <a
                      href={`http://localhost:5000/contest/${item.id}`}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-blue-600"
                    >
                      Place Bet
                    </a>
                  ) : columnKey === "startTimeSeconds" ? (
                    `${secondsToDate(item.startTimeSeconds)}`
                  ) : (
                    getKeyValue(item, columnKey)
                  )}
                </TableCell>
              )}
            </TableRow>
          )}
        </TableBody>
      </Table>
    </>
  );
}
