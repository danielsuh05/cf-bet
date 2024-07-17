import { useLocation } from "react-router-dom";
import {
  Navbar,
  NavbarBrand,
  NavbarContent,
  NavbarItem,
  Link,
} from "@nextui-org/react";
import LoginModal from "./LoginModal";

export default function NavBar() {
  const location = useLocation();
  const { pathname } = location;

  return (
    <Navbar className="mb-5" isBordered>
      <NavbarBrand>
        <Link href="/" className="font-bold text-inherit">
          cf-bet
        </Link>
      </NavbarBrand>
      <NavbarContent className="hidden sm:flex gap-4" justify="center">
        <NavbarItem isActive={pathname === "/"}>
          <Link color="foreground" href="/">
            Home
          </Link>
        </NavbarItem>
        <NavbarItem isActive={pathname === "/contests"}>
          <Link color="foreground" href="/contests">
            Contests
          </Link>
        </NavbarItem>
        <NavbarItem isActive={pathname === "/mybets"}>
          <Link color="foreground" href="/mybets">
            My Bets
          </Link>
        </NavbarItem>
        <NavbarItem isActive={pathname === "/rankings"}>
          <Link color="foreground" href="/rankings">
            Rankings
          </Link>
        </NavbarItem>
      </NavbarContent>
      <NavbarContent justify="end">
        <NavbarItem>
          <LoginModal />
        </NavbarItem>
      </NavbarContent>
    </Navbar>
  );
}
