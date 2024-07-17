import ContestsTable from "../components/ContestsTable";
import NavBar from "../components/NavBar";

function Contests() {
  return (
    <>
      <NavBar />
      <div className="ml-48 mr-48">
        <ContestsTable />
      </div>
    </>
  );
}

export default Contests;
