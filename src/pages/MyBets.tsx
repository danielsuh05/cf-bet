import MyBetsTable from "../components/MyBetsTable";
import NavBar from "../components/NavBar";

function MyBets() {
  return (
    <>
      <NavBar />
      <div className="ml-48 mr-48">
        <MyBetsTable />
      </div>
    </>
  );
}

export default MyBets;
