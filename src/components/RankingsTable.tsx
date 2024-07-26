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
import { numberWithCommas } from "../utils/utils";

const columns = [
  {
    key: "username",
    label: "USERNAME",
  },
  {
    key: "moneyBalance",
    label: "BALANCE",
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

  console.log(users);

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
                <TableCell>
                  {columnKey === "username" ? (
                    <a
                      href={`http://localhost:5000/user/${getKeyValue(
                        item,
                        columnKey
                      )}`}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-blue-600"
                    >
                      {getKeyValue(item, columnKey)}
                    </a>
                  ) : (
                    "$" +
                    numberWithCommas(
                      parseFloat(
                        parseFloat(getKeyValue(item, columnKey)).toFixed(2)
                      )
                    )
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
