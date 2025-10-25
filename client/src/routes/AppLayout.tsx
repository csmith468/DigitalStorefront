import { Outlet } from "react-router-dom";
import CategorySidebar from "../components/common/CategorySidebar";
import { Header } from "../components/common/Header";

export function AppLayout() {
  return (
    <div className="app">
      <Header />
      <div className="app-layout">
        <CategorySidebar />
        <main className="app-main">
          <Outlet />
        </main>
      </div>
    </div>
  );
}