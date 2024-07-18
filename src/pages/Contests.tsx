import ContestsTable from "../components/ContestsTable";
import NavBar from "../components/NavBar";

function Contests() {
  return (
    <>
      <NavBar />
      <div className="flex items-center justify-center w-full">
        <div className="w-full max-w-[1200px]">
          <ContestsTable />
        </div>
      </div>
    </>
  );
}

export default Contests;
