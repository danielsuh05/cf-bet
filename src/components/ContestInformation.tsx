import { Navbar } from "@nextui-org/navbar";
import { useParams } from "react-router-dom";

export default function ContestInformation() {
  const { contestId } = useParams();

  return (
    <>
      <Navbar />
    </>
  );
}
