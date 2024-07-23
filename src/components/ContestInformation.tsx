import { useCookies } from "react-cookie";
import {
  RadioGroup,
  Radio,
  cn,
  Select,
  SelectItem,
  Input,
  Divider,
  Button,
  CircularProgress,
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
  placeCompeteBet,
  placeTopNBet,
  placeWinnerBet,
} from "../services/betService";
import { getUserInfo } from "../services/userService";
import { numberWithCommas } from "../utils/utils";
import MyBetsTable from "./MyBetsTable";

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
      label="Competitor to Beat"
      placeholder="Select another Competitor"
      className="max-w-xs"
      onChange={onCompetitorChange}
      selectedKeys={[selectedCompetitor]}
    >
      {competitors.map((competitor: any) => (
        <SelectItem key={competitor.handle}>{competitor.handle}</SelectItem>
      ))}
    </Select>
  );
};

export const BetInfo = (props: any) => {
  const {
    selectedBetOption,
    selectedCompetitor,
    topNValue,
    selectedRow,
    username,
    token,
    contestId,
    setMoneyBalance,
    moneyBalance,
  } = props;

  const [odds, setOdds] = useState("");

  useEffect(() => {
    async function fetchProbability() {
      try {
        const request: any = {
          username: username,
          contestId: contestId,

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

        setOdds(line);

        const user = await getUserInfo(username);
        setMoneyBalance(user.moneyBalance);
      } catch (error) {
        console.error("Error fetching contests:", error);
      }
    }
    fetchProbability();
  }, [
    contestId,
    selectedBetOption,
    selectedCompetitor,
    selectedRow,
    setMoneyBalance,
    token,
    topNValue,
    username,
  ]);

  return (
    <>
      <div className="flex flex-col h-5 items-center text-large mt-4 gap-5 ml-2 mr-2">
        <div className="flex flex-row">
          Odds:
          <div className={odds[0] === "-" ? "text-red-600" : "text-green-600"}>
            &nbsp;{(odds[0] === "-" ? "" : "+") + odds}
          </div>
        </div>
        <Divider
          orientation="horizontal"
          className="max-w-[300px] min-w-[250px]"
        />
        <div className="flex flex-row">
          Your Balance:
          <div>
            &nbsp;${numberWithCommas(parseFloat(moneyBalance.toFixed(2)))}
          </div>
        </div>
      </div>
    </>
  );
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
  const [betAmount, setBetAmount] = useState<string>("");
  const [errorMessage, setErrorMessage] = useState<string>("");
  const [successMessage, setSuccessMessage] = useState<string>("");
  const [loader, setLoader] = useState<boolean>(false);
  const [moneyBalance, setMoneyBalance] = useState<number>(-1);
  const [refreshBets, setRefreshBets] = useState<boolean>(false);

  const placeBet = async () => {
    setLoader(true);
    async function tryPlaceBet() {
      try {
        const request: any = {
          username: username,
          contestId: contestId,
          initialBet: parseFloat(betAmount),

          betHandle1: selectedRow,
          betHandle2: selectedCompetitor,

          topNBetHandle: selectedRow,
          ranking: topNValue,

          winnerBetHandle: selectedRow,
        };

        if (selectedBetOption === "compete") {
          await placeCompeteBet(jwtToken, request);
        } else if (selectedBetOption === "topn") {
          await placeTopNBet(jwtToken, request);
        } else {
          await placeWinnerBet(jwtToken, request);
        }

        setErrorMessage("");
        setSuccessMessage("Bet placed successfully!");

        setTimeout(() => {
          setSuccessMessage("");
        }, 5000);
      } catch (error: any) {
        setErrorMessage(error.response.data.detail);
      }
    }

    await tryPlaceBet();

    const user = await getUserInfo(username);
    setMoneyBalance(user.moneyBalance);
    setRefreshBets(!refreshBets);

    setLoader(false);
  };

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

  const handleCompetitorChange = (e: any) => {
    setSelectedCompetitor(e.target.value);
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
              {jwtToken === undefined ? (
                <Card className="p-5">
                  <div className="font-semibold text-3xl mb-5">
                    Please log in.
                  </div>
                </Card>
              ) : (
                <Card>
                  <CardBody className="p-5">
                    <div className="font-semibold text-3xl mb-5">
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

                    {((selectedBetOption === "compete" &&
                      selectedCompetitor !== "") ||
                      (selectedBetOption === "topn" &&
                        topNValue > 0 &&
                        topNValue <= 250) ||
                      selectedBetOption === "winner") &&
                    selectedRow !== null ? (
                      <div className="inline-grid grid-cols-3 justify-items-center items-center pl-5 pr-5 mt-5">
                        <Card className="w-fit flex items-center justify-center mt-5">
                          <CardBody className="min-h-[170px] w-fit">
                            <BetInfo
                              selectedBetOption={selectedBetOption}
                              selectedCompetitor={selectedCompetitor}
                              topNValue={topNValue}
                              selectedRow={selectedRow}
                              username={username}
                              token={jwtToken}
                              contestId={contestId}
                              moneyBalance={moneyBalance}
                              setMoneyBalance={setMoneyBalance}
                            />
                          </CardBody>
                        </Card>
                        <Divider orientation="vertical" className="h-[70%]" />
                        <div className="flex flex-col items-center justify-items-center">
                          <Input
                            type="number"
                            label="Bet Amount"
                            placeholder="0.00"
                            labelPlacement="outside"
                            value={betAmount}
                            onValueChange={setBetAmount}
                            isInvalid={
                              parseFloat(betAmount) <= 0 ||
                              parseFloat(betAmount) > moneyBalance ||
                              moneyBalance === -1
                            }
                            startContent={
                              <div className="pointer-events-none flex items-center">
                                <span className="text-default-400 text-small">
                                  $
                                </span>
                              </div>
                            }
                          />
                          <Button
                            onPress={placeBet}
                            color="danger"
                            variant="flat"
                            className="font-bold mt-5 "
                          >
                            <span>Place Bet</span>
                            {loader && (
                              <CircularProgress
                                color="danger"
                                aria-label="Loading..."
                                size="sm"
                              />
                            )}
                          </Button>
                          {errorMessage != "" && (
                            <p style={{ color: "#f31260" }}>{errorMessage}</p>
                          )}
                          {errorMessage === "" && successMessage != "" && (
                            <p
                              style={{ color: "#16a34a", marginTop: "0.5rem" }}
                            >
                              {successMessage}
                            </p>
                          )}
                        </div>
                      </div>
                    ) : (
                      ""
                    )}
                  </CardBody>
                </Card>
              )}
              {jwtToken === undefined ? (
                ""
              ) : (
                <Card className="mt-5">
                  <CardBody className="p-5">
                    <div className="font-semibold text-2xl mb-5">Your Bets</div>
                    <MyBetsTable
                      username={username}
                      contestId={parseInt(contestId!)}
                      removeWrapper={true}
                      refreshBets={refreshBets}
                    />
                  </CardBody>
                </Card>
              )}
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
