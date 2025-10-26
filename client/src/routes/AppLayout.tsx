import { Outlet } from "react-router-dom";
import CategorySidebar from "../components/common/CategorySidebar";
import { Header } from "../components/common/Header";
import { useState } from "react";

export function AppLayout() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  return (
    <div className="app">
      <Header mobileMenuOpen={mobileMenuOpen} setMobileMenuOpen={setMobileMenuOpen} />
      <div className="app-layout">
        <CategorySidebar mobileMenuOpen={mobileMenuOpen} setMobileMenuOpen={setMobileMenuOpen} />
        <main className="app-main">
          <Outlet />
        </main>
      </div>
    </div>
  );
}