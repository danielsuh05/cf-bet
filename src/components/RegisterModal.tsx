import {
  Modal,
  ModalContent,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
  useDisclosure,
  Input,
  Link,
} from "@nextui-org/react";
import { useState } from "react";
import { useCookies } from "react-cookie";
import { register } from "../services/userService";

export default function RegisterModal() {
  const { isOpen, onOpen, onOpenChange } = useDisclosure();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [, setJwtCookie] = useCookies(["jwt"]);
  const [, setUsernameCookie] = useCookies(["username"]);

  const handleRegister = async () => {
    try {
      const response = await register(username, password);

      if (response == undefined) {
        throw new Error(response.detail);
      }

      // Calculate expiration time, e.g., 7 days from now
      const expirationDate = new Date();
      expirationDate.setDate(expirationDate.getDate() + 14);

      setJwtCookie("jwt", response.token, {
        path: "/",
        expires: expirationDate,
      });
      setUsernameCookie("username", response.username, {
        path: "/",
        expires: expirationDate,
      });

      // Close the modal
      onOpenChange();
      setError("");
    } catch (error: any) {
      setError(error.response.data.detail);
    }
  };

  return (
    <>
      <Button
        onPress={onOpen}
        color="secondary"
        variant="flat"
        className="font-bold"
      >
        Register
      </Button>
      <Modal isOpen={isOpen} onOpenChange={onOpenChange} placement="top-center">
        <ModalContent>
          {(onClose) => (
            <>
              <ModalHeader className="flex flex-col gap-1">
                Register
              </ModalHeader>
              <ModalBody>
                <Input
                  autoFocus
                  label="Username"
                  placeholder="Enter your username"
                  variant="bordered"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                />
                <Input
                  label="Password"
                  placeholder="Enter your password"
                  type="password"
                  variant="bordered"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                />
                <div className="flex py-2 px-1 justify-between">
                  <Link color="primary" href="#" size="sm">
                    Forgot password?
                  </Link>
                </div>
                {error != "" && <p style={{ color: "red" }}>{error}</p>}
              </ModalBody>
              <ModalFooter>
                <Button
                  color="danger"
                  variant="flat"
                  onPress={() => {
                    onClose();
                    setError("");
                  }}
                >
                  Close
                </Button>
                <Button color="primary" onPress={handleRegister}>
                  Register
                </Button>
              </ModalFooter>
            </>
          )}
        </ModalContent>
      </Modal>
    </>
  );
}
