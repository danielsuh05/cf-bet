import { useCookies } from "react-cookie";
import {
  RadioGroup,
  Radio,
  cn,
  Select,
  SelectItem,
  Input,
} from "@nextui-org/react";
import { useParams } from "react-router-dom";
import NavBar from "./NavBar";
import { useEffect, useState } from "react";
import { getCompetitors } from "../services/contestService";
import {
  Card,
  CardBody,
  getKeyValue,
  Table,
  TableBody,
  TableCell,
  TableColumn,
  TableHeader,
  TableRow,
} from "@nextui-org/react";
import {
  getCompeteBetDetails,
  getTopNBetDetails,
  getWinnerBetDetails,
} from "../services/betService";

const columns = [
  {
    key: "handle",
    label: "HANDLE",
  },
  {
    key: "ranking",
    label: "RATING",
  },
];

export const CustomRadio = (props: any) => {
  const { children, ...otherProps } = props;

  return (
    <Radio
      {...otherProps}
      classNames={{
        base: cn(
          "inline-flex m-0 bg-content1 hover:bg-content2 items-center justify-between",
          "flex-row-reverse max-w-[250px] cursor-pointer rounded-lg gap-4 p-4 border-2 border-transparent",
          "data-[selected=true]:border-primary"
        ),
      }}
    >
      {children}
    </Radio>
  );
};

export const TopN = (props: any) => {
  const { value, onValueChange } = props;

  return (
    <Input
      type="number"
      label="Top"
      placeholder="rank"
      labelPlacement="outside"
      isInvalid={value <= 0 || value > 250}
      errorMessage="Please enter a number between 1-250."
      value={value === -100 ? 1 : value}
      onValueChange={onValueChange}
    />
  );
};

export const Compete = (props: any) => {
  const { competitors, selectedCompetitor, onCompetitorChange } = props;

  return (
    <Select
      items={competitors}
      label="Competitor to Beat"
      placeholder="Select another Competitor"
      className="max-w-xs"
      onChange={onCompetitorChange}
      selectedKeys={selectedCompetitor}
    >
      {(competitor: any) => (
        <SelectItem key={competitor.handle} value={competitor.handle}>
          {competitor.handle}
        </SelectItem>
      )}
    </Select>
  );
};

export const SubmitBet = (props: any) => {
  const {
    selectedBetOption,
    selectedCompetitor,
    topNValue,
    selectedRow,
    username,
    token,
  } = props;

  useEffect(() => {
    async function fetchUserRankings() {
      try {
        const request = {
          username: username,

          betHandle1: selectedRow,
          betHandle2: selectedCompetitor,

          topNBetHandle: selectedRow,
          ranking: topNValue,

          winnerBetHandle: selectedRow,
        };

        console.log(request);

        let line: string;

        if (selectedBetOption === "compete") {
          line = await getCompeteBetDetails(token, request);
        } else if (selectedBetOption === "topn") {
          line = await getTopNBetDetails(token, request);
        } else {
          line = await getWinnerBetDetails(token, request);
        }

        console.log(line);
      } catch (error) {
        console.error("Error fetching contests:", error);
      }
    }
    fetchUserRankings();
  }, [
    selectedBetOption,
    selectedCompetitor,
    selectedRow,
    token,
    topNValue,
    username,
  ]);

  return <></>;
};

export default function ContestInformation() {
  const { contestId } = useParams();
  const [usernameCookies] = useCookies(["username"]);
  const username = usernameCookies.username;

  const [jwtCookies] = useCookies(["jwt"]);
  const jwtToken = jwtCookies.jwt;

  const [competitors, setCompetitors] = useState([]);
  const [selectedRow, setSelectedRow] = useState<any>(null);
  const [selectedBetOption, setSelectedBetOption] = useState<string>("");
  const [selectedCompetitor, setSelectedCompetitor] = useState<string>("");
  const [topNValue, setTopNValue] = useState<any>(-100);

  useEffect(() => {
    async function fetchUserRankings() {
      try {
        if (contestId === undefined) {
          throw new Error("ContestID is invalid.");
        }
        const response = await getCompetitors(parseInt(contestId));
        console.log(response);
        setCompetitors(response.competitors);
      } catch (error) {
        console.error("Error fetching contests:", error);
      }
    }
    fetchUserRankings();
  }, [contestId]);

  const handleSelectionChange = (keys: any) => {
    const selectedKey = Array.from(keys)[0];
    setSelectedRow(selectedKey);
  };

  const handleChangeBetOption = (value: string) => {
    setSelectedBetOption(value);
  };

  const handleCompetitorChange = (value: string) => {
    setSelectedCompetitor(value);
  };

  const handleTopNChange = (value: number) => {
    setTopNValue(value);
  };

  return (
    <>
      <NavBar />
      <div className="flex items-center justify-center w-full h-[89.2vh]">
        <div className="w-full max-w-[1200px] h-full">
          <div className="grid grid-cols-1 md:grid-cols-[1fr_70%] gap-2 h-full">
            <div className="h-full">
              <Table
                selectionMode="single"
                color="primary"
                className="max-h-[93.2vh]"
                onSelectionChange={handleSelectionChange}
              >
                <TableHeader columns={columns}>
                  {(column) => (
                    <TableColumn key={column.key}>{column.label}</TableColumn>
                  )}
                </TableHeader>
                <TableBody
                  items={competitors}
                  emptyContent="No competitors currently."
                >
                  {(item: any) => (
                    <TableRow key={item.handle}>
                      {(columnKey) => (
                        <TableCell>{getKeyValue(item, columnKey)}</TableCell>
                      )}
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </div>
            <div className="h-full">
              <Card>
                <CardBody className="p-5">
                  <div className="font-semibold text-2xl mb-5">
                    {selectedRow !== null
                      ? `Betting on ${selectedRow}:`
                      : "Select a User"}
                  </div>
                  {selectedRow !== null ? (
                    <RadioGroup
                      label="Type of Bet"
                      orientation="horizontal"
                      onValueChange={handleChangeBetOption}
                    >
                      <CustomRadio
                        description="User will place higher than another."
                        value="compete"
                      >
                        Compete
                      </CustomRadio>
                      <CustomRadio
                        description="User will place in the top N competitors."
                        value="topn"
                      >
                        Top N
                      </CustomRadio>
                      <CustomRadio
                        description="User will win the contest."
                        value="winner"
                      >
                        Winner
                      </CustomRadio>
                    </RadioGroup>
                  ) : (
                    ""
                  )}
                  {selectedBetOption === "compete" && selectedRow !== null ? (
                    <div className="mt-10">
                      <div className="flex items-center justify-center">
                        <p>{selectedRow} will beat &nbsp;</p>
                        <Compete
                          competitors={competitors}
                          selectedCompetitor={selectedCompetitor}
                          onCompetitorChange={handleCompetitorChange}
                        />
                      </div>
                    </div>
                  ) : selectedBetOption === "topn" && selectedRow !== null ? (
                    <div className="flex items-center justify-center mt-10">
                      <TopN
                        value={topNValue}
                        onValueChange={handleTopNChange}
                      />
                    </div>
                  ) : (
                    ""
                  )}
                </CardBody>
                {((selectedBetOption === "compete" &&
                  selectedCompetitor !== "") ||
                  (selectedBetOption === "topn" &&
                    topNValue > 0 &&
                    topNValue <= 250) ||
                  selectedBetOption === "winner") &&
                selectedRow !== null ? (
                  <SubmitBet
                    selectedBetOption={selectedBetOption}
                    selectedCompetitor={selectedCompetitor}
                    topNValue={topNValue}
                    selectedRow={selectedRow}
                    username={username}
                    token={jwtToken}
                  />
                ) : (
                  ""
                )}
              </Card>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
