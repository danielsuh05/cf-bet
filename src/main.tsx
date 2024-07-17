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

ReactDOM.createRoot(document.getElementById("root")!).render(
  <Router>
    <React.StrictMode>
      <NextUIProvider>
        <Routes>
          <Route path="/" element={<App />} />
          <Route path="/contests" element={<Contests />} />
          <Route path="/mybets" element={<MyBets />} />
          <Route path="/rankings" element={<Rankings />} />
        </Routes>
      </NextUIProvider>
    </React.StrictMode>
  </Router>
);
