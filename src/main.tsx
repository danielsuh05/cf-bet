// main.tsx or main.jsx
import React from "react";
import ReactDOM from "react-dom/client";
import { NextUIProvider } from "@nextui-org/react";
import App from "./pages/App";
import Contests from "./pages/Contests";
import MyBets from "./pages/MyBets";
import Rankings from "./pages/Rankings";
import "./index.css";
import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import UserProfile from "./components/UserProfile";
import ContestInformation from "./components/ContestInformation";
import { CookiesProvider } from "react-cookie";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <CookiesProvider>
    <Router>
      <React.StrictMode>
        <NextUIProvider>
          <Routes>
            <Route path="/" element={<App />} />
            <Route path="/contests" element={<Contests />} />
            <Route path="/mybets" element={<MyBets />} />
            <Route path="/rankings" element={<Rankings />} />
            <Route path="/user/:username" element={<UserProfile />} />
            <Route
              path="/contest/:contestId"
              element={<ContestInformation />}
            />
          </Routes>
        </NextUIProvider>
      </React.StrictMode>
    </Router>
  </CookiesProvider>
);
