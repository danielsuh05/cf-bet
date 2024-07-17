import NavBar from "../components/NavBar";
import RankingsTable from "../components/RankingsTable";

function Rankings() {
  return (
    <>
      <NavBar />
      <div className="flex justify-center items-center">
        <div className="w-1/3">
          <RankingsTable />
        </div>
      </div>
    </>
  );
}

export default Rankings;
