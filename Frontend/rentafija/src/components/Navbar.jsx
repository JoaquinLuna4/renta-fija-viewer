import React, { useState } from "react";
import {
	AppBar,
	Toolbar,
	Typography,
	Button,
	IconButton,
	Drawer,
	List,
	ListItem,
	ListItemText,
	useMediaQuery,
	useTheme,
	Box,
	Container,
} from "@mui/material";
import MenuIcon from "@mui/icons-material/Menu";
import { Link } from "react-router-dom";

const Navbar = () => {
	const [mobileOpen, setMobileOpen] = useState(false);
	const theme = useTheme();
	const isMobile = useMediaQuery(theme.breakpoints.down("md"));

	const handleDrawerToggle = () => {
		setMobileOpen(!mobileOpen);
	};

	const navItems = [
		{ text: "Inicio", path: "/" },
		{ text: "Activos", path: "/allAssets" },
		{ text: "Acerca de", path: "/about" },
	];

	const drawer = (
		<Box onClick={handleDrawerToggle} sx={{ textAlign: "center", width: 250 }}>
			<Typography variant="h6" sx={{ my: 2 }}>
				Rentapp
			</Typography>
			<List>
				{navItems.map((item) => (
					<ListItem
						key={item.text}
						component={Link}
						to={item.path}
						sx={{
							color: "primary.main",
							"&:hover": {
								backgroundColor: "action.hover",
							},
						}}
					>
						<ListItemText primary={item.text} />
					</ListItem>
				))}
			</List>
		</Box>
	);

	return (
		<AppBar position="static" color="primary" elevation={0}>
			<Container maxWidth={false}>
				<Toolbar disableGutters>
					<Typography
						variant="h6"
						component={Link}
						to="/"
						sx={{
							flexGrow: 1,
							fontWeight: 700,
							letterSpacing: ".1rem",
							textDecoration: "none",
							color: "inherit",
						}}
					>
						Renta Fija
					</Typography>

					{isMobile ? (
						<IconButton
							color="inherit"
							aria-label="open drawer"
							edge="end"
							onClick={handleDrawerToggle}
							sx={{ ml: "auto" }}
						>
							<MenuIcon />
						</IconButton>
					) : (
						<Box sx={{ display: "flex" }}>
							{navItems.map((item) => (
								<Button
									key={item.text}
									component={Link}
									to={item.path}
									sx={{
										color: "white",
										mx: 1,
										"&:hover": {
											backgroundColor: "rgba(255, 255, 255, 0.1)",
										},
									}}
								>
									{item.text}
								</Button>
							))}
						</Box>
					)}
				</Toolbar>
			</Container>

			<Drawer
				variant="temporary"
				anchor="right"
				open={mobileOpen}
				onClose={handleDrawerToggle}
				ModalProps={{
					keepMounted: true, // Better open performance on mobile
				}}
				sx={{
					display: { xs: "block", md: "none" },
					"& .MuiDrawer-paper": { boxSizing: "border-box", width: 250 },
				}}
			>
				{drawer}
			</Drawer>
		</AppBar>
	);
};

export default Navbar;
