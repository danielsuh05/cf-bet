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
import { secondsToDate } from "../utils/utils";

const columns = [
  {
    key: "details",
    label: "DETAILS",
  },
  {
    key: "contestId",
    label: "CONTEST ID",
  },
  {
    key: "status",
    label: "HIT?",
  },
  {
    key: "date",
    label: "DATE",
  },
  {
    key: "profitLoss",
    label: "PROFIT/LOSS",
  },
];

export default function MyBetsTable({ username }: { username: any }) {
  const [bets, setBets] = useState([]);

  useEffect(() => {
    async function fetchMyBets() {
      try {
        const response = await getUserBets(username);
        setBets(response);
      } catch (error) {
        console.error("Error fetching contests:", error);
      }
    }
    fetchMyBets();
  }, [username]);

  const getBetString = (item: any) => {
    if (item.betType === 0) {
      // compete
      return `${item.betHandle1} will beat ${item.betHandle2}`;
    } else if (item.betType === 1) {
      // top n
      return `${item.topNBetHandle} will be top ${item.topNBetHandle}`;
    } else {
      // winner
      return `${item.winnerBetHandle} will win the contest`;
    }
  };

  const getStatus = (item: any) => {
    if (item.status === 0) {
      return "Hit";
    } else if (item.status === 1) {
      return "Miss";
    } else if (item.status === 2) {
      return "Pending";
    } else {
      return "Invalid";
    }
  };

  return (
    <>
      <Table selectionMode="single" isStriped color="default">
        <TableHeader columns={columns}>
          {(column) => (
            <TableColumn key={column.key}>{column.label}</TableColumn>
          )}
        </TableHeader>
        <TableBody items={bets} emptyContent="No bets.">
          {(item: any) => (
            <TableRow key={item.relativeTimeSeconds}>
              {(columnKey) => (
                <TableCell>
                  {columnKey === "details" ? (
                    getBetString(item)
                  ) : columnKey === "date" ? (
                    secondsToDate(item)
                  ) : columnKey === "profitLoss" ? (
                    <div
                      className={
                        getKeyValue(item, columnKey) <= 0
                          ? "text-red-600"
                          : "text-green-600"
                      }
                    >
                      {getKeyValue(item, columnKey)}
                    </div>
                  ) : columnKey === "status" ? (
                    <div
                      className={
                        getKeyValue(item, columnKey) == 1
                          ? "text-red-600"
                          : getKeyValue(item, columnKey) == 0
                          ? "text-green-600"
                          : "text-black"
                      }
                    >
                      {getStatus(item)}
                    </div>
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
