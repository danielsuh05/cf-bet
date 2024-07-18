import NavBar from "../components/NavBar";
import RankingsTable from "../components/RankingsTable";

function Rankings() {
  return (
    <>
      <NavBar />
      <div className="flex justify-center items-center">
        <div className="max-w-[700px] w-full">
          <RankingsTable />
        </div>
      </div>
    </>
  );
}

export default Rankings;
