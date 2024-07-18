import { useCookies } from "react-cookie";
import ContestsTable from "../components/ContestsTable";
import MyBetsTable from "../components/MyBetsTable";
import NavBar from "../components/NavBar";
import RankingsTable from "../components/RankingsTable";

function App() {
  const [usernameCookies] = useCookies(["username"]);
  const username = usernameCookies.username;

  return (
    <>
      <NavBar />
      <div className="flex items-center justify-center w-full">
        <div className="w-full max-w-[1200px]">
          <div className="grid grid-cols-2 h-[90vh]">
            <div className="p-1 m-1 rounded-[14px] hover:bg-black/[0.03] transition-all h-fit">
              <MyBetsTable username={username} />
            </div>
            <div className="grid grid-rows-2 h-fit">
              <div className="p-1 m-1 rounded-[14px] hover:bg-black/[0.03] transition-all h-fit">
                <ContestsTable />
              </div>
              <div className="p-1 m-1 rounded-[14px] hover:bg-black/[0.03] transition-all h-fit">
                <RankingsTable />
              </div>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}

export default App;
