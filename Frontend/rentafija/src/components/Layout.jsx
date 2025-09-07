import { Outlet } from "react-router-dom";
import Navbar from "./Navbar";
import FooterMui from "./FooterMui";
import { Box } from "@mui/material";
function Layout() {
	return (
		<Box
			sx={{
				display: "flex",
				flexDirection: "column",
				minHeight: "100vh",
			}}
		>
			<Navbar />
			<Box component="main" sx={{ flexGrow: 1 }}>
				<Outlet />
			</Box>
			<FooterMui />
		</Box>
	);
}

export default Layout;
