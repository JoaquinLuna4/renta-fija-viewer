import { Routes, Route } from "react-router-dom";
import Layout from "../components/Layout";
import Home from "../views/Home";
import AllAssets from "../views/AllAssets";
function AppRoutes() {
	return (
		<Routes>
			<Route element={<Layout />}>
				<Route path="/" element={<Home />} />
				<Route path="/allAssets" element={<AllAssets />} />
			</Route>
			{/* <Route path="/success" element={<Success />} />
			<Route path="/login" element={<Login />} /> */}
			{/* Uncomment if you want to use these routes */}
		</Routes>
	);
}

export default AppRoutes;
