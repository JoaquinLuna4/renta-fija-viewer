import { Outlet } from "react-router-dom";

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
			{/* <Navbar /> */} {/* Uncomment if you want to use the Navbar */}
			<Box component="main" sx={{ flexGrow: 1 }}>
				<Outlet />
			</Box>
			<FooterMui />
		</Box>
	);
}

export default Layout;
