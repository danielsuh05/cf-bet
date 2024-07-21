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
import { getUserBets, getUserContestBets } from "../services/userService";

const columns = [
  {
    key: "details",
    label: "DETAILS",
  },
  {
    key: "initialBet",
    label: "INITIAL BET",
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
    key: "profitLoss",
    label: "PROFIT/LOSS",
  },
];

export default function MyBetsTable({
  username,
  contestId = -1,
  removeWrapper = false,
  refreshBets = false,
}: {
  username: any;
  contestId?: any;
  removeWrapper?: boolean;
  refreshBets?: boolean;
}) {
  const [bets, setBets] = useState([]);

  useEffect(() => {
    async function fetchMyBets() {
      try {
        let response: any = null;
        if (contestId === -1) {
          response = await getUserBets(username);
        } else {
          response = await getUserContestBets(username, contestId);
        }
        if (response === null) throw new Error();
        setBets(response);
      } catch (error) {
        console.error("Error fetching contests:", error);
      }
    }
    fetchMyBets();
  }, [username, contestId, refreshBets]);

  const getBetString = (item: any) => {
    if (item.betType === 0) {
      // compete
      return `${item.betHandle1} will beat ${item.betHandle2}`;
    } else if (item.betType === 1) {
      // top n
      return `${item.topNBetHandle} will be top ${item.ranking}`;
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
      <Table
        selectionMode="single"
        isStriped
        color="default"
        removeWrapper={removeWrapper}
        className={removeWrapper ? "p-[1rem]" : "p-[0px]"}
      >
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
                  ) : columnKey === "profitLoss" ? (
                    <div
                      className={
                        getKeyValue(item, columnKey) <= 0
                          ? "text-red-600"
                          : "text-green-600"
                      }
                    >
                      {getKeyValue(item, columnKey) !== null
                        ? parseFloat(getKeyValue(item, columnKey)).toFixed(2)
                        : ""}
                    </div>
                  ) : columnKey === "initialBet" ? (
                    <div>
                      {getKeyValue(item, columnKey) !== null
                        ? parseFloat(getKeyValue(item, columnKey)).toFixed(2)
                        : ""}
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
