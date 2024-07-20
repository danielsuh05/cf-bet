import { useParams } from "react-router-dom";
import Chart from "react-apexcharts";
import NavBar from "./NavBar";
import MyBetsTable from "./MyBetsTable";
import { useEffect, useState } from "react";
import { getUserBets, getUserInfo } from "../services/userService";
import { numberWithCommas } from "../utils/utils";
import { ApexOptions } from "apexcharts";

export default function UserProfile() {
  const { username } = useParams();
  const [plProgression, setPlProgression] = useState<number[]>([]);
  const [user, setUser] = useState<any>();

  useEffect(() => {
    async function fetchUserRankings() {
      try {
        if (username == undefined) {
          throw new Error("No username provided.");
        }
        const response = await getUserBets(username);

        let curPl = 1000;
        let progression = response.map((bet: { profitLoss: number }) => {
          curPl += bet.profitLoss;
          return curPl;
        });

        progression = [1000, ...progression];
        setPlProgression(progression);
      } catch (error) {
        console.error("Error fetching contests:", error);
      }
    }

    async function getUser() {
      try {
        if (username == undefined) {
          throw new Error("No username provided.");
        }
        const response = await getUserInfo(username);

        setUser(response);
      } catch (error) {
        console.error("Error fetching user", username);
      }
    }

    fetchUserRankings();
    getUser();
  }, [username]);

  const Profile = () => {
    return (
      user != undefined && (
        <div className="p-2">
          <h1 className="font-bold text-3xl mb-3">{username}</h1>
          {/* todo */}
          <p className="tracking-widest underline">NET WORTH</p>
          <h1 className="font-bold text-2xl mb-3">
            ${numberWithCommas(user.moneyBalance)}
          </h1>
          <p className="tracking-widest underline">HIT %</p>
          <h1 className="font-bold text-2xl mb-3">
            {user.hitPercentage.toFixed(2)}%
          </h1>
          <p className="tracking-widest underline">RANK</p>
          <h1 className="font-bold text-2xl">{user.rank}</h1>
        </div>
      )
    );
  };

  const Graph = () => {
    const graphOptions: ApexOptions = {
      title: {
        text: "Net Worth over Time",
        align: "center",
        style: {
          fontSize: "20px",
          fontFamily: "Inter, sans-serif",
        },
      },
      xaxis: {
        title: {
          text: "Bet Number",
          offsetX: 0,
          offsetY: 0,
          style: {
            fontSize: "12px",
            fontFamily: "Inter, sans-serif",
            fontWeight: 600,
            cssClass: "apexcharts-xaxis-title",
          },
        },
      },
      yaxis: {
        title: {
          text: "Net Worth",
          offsetX: 0,
          offsetY: 0,
          style: {
            fontSize: "12px",
            fontFamily: "Inter, sans-serif",
            fontWeight: 600,
            cssClass: "apexcharts-yaxis-title",
          },
        },
        labels: {
          formatter: (value: number) => {
            return `$${numberWithCommas(value)}`;
          },
        },
      },
      chart: {
        animations: {
          enabled: true,
          easing: "easeout",
          speed: 300,
        },
        foreColor: "var(--nextui-colors-accents9)",
      },
      series: [
        {
          name: "Net Worth",
          data: plProgression,
        },
      ],
    };

    return (
      <div className="app">
        <div className="row">
          <div className="mixed-chart">
            {
              <Chart
                options={graphOptions}
                series={graphOptions.series}
                type="line"
              />
            }
          </div>
        </div>
      </div>
    );
  };

  return (
    <>
      <NavBar />
      <div className="flex items-center justify-center w-full">
        <div className="w-full max-w-[1200px]">
          <div className="grid grid-cols-1 md:grid-cols-[1fr_70%] gap-2">
            <div className="border-2 p-1 m-1 rounded-[14px] hover:bg-black/[0.03] transition-all h-fit">
              <Profile />
            </div>
            <div className="border-2 p-1 m-1 rounded-[14px] hover:bg-black/[0.03] transition-all h-fit">
              <Graph />
            </div>
          </div>
          <div className="mt-4">
            <MyBetsTable username={username} />
          </div>
        </div>
      </div>
    </>
  );
}
