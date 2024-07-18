import { useLocation } from "react-router-dom";
import {
  Navbar,
  NavbarBrand,
  NavbarContent,
  NavbarItem,
  Link,
} from "@nextui-org/react";
import LoginModal from "./LoginModal";
import RegisterModal from "./RegisterModal";
import { useCookies } from "react-cookie";

export default function NavBar() {
  const location = useLocation();
  const { pathname } = location;

  const [jwtCookies] = useCookies(["jwt"]);
  const jwtToken = jwtCookies.jwt;

  const [usernameCookies] = useCookies(["username"]);
  const username = usernameCookies.username;

  const UserProfileButton = () => {
    return (
      <a
        href={`http://localhost:8000/user/${username}`}
        target="_blank"
        rel="noopener noreferrer"
        className="text-blue-600 underline"
      >
        {username}
      </a>
    );
  };

  return (
    <Navbar className="mb-3" isBordered maxWidth="xl">
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
        {jwtToken == undefined && <LoginModal />}
        {jwtToken == undefined && <RegisterModal />}
        {jwtToken != undefined && <UserProfileButton />}
      </NavbarContent>
    </Navbar>
  );
}
