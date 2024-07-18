import { useCookies } from "react-cookie";
import MyBetsTable from "../components/MyBetsTable";
import NavBar from "../components/NavBar";

function MyBets() {
  const [usernameCookies] = useCookies(["username"]);
  const username = usernameCookies.username;

  return (
    <>
      <NavBar />
      <div className="flex items-center justify-center w-full">
        <div className="w-full max-w-[1200px]">
          <MyBetsTable username={username} />
        </div>
      </div>
    </>
  );
}

export default MyBets;
