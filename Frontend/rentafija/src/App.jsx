import { Container } from "@mui/material";
import AllAssets from "./views/AllAssets";
import CssBaseline from "@mui/material/CssBaseline";
import Box from "@mui/material/Box";
import lightTheme from "./themes/lightTheme.js";
import { ThemeProvider } from "@mui/material/styles";
import { useState } from "react";
import Typography from "@mui/material/Typography";
import Button from "@mui/material/Button";

function App() {
	const [mode, setMode] = useState("light");

	// Función para alternar entre modos
	const toggleColorMode = () => {
		setMode((prevMode) => (prevMode === "light" ? "dark" : "light"));
	};

	return (
		<ThemeProvider theme={lightTheme}>
			<CssBaseline />
			<Container
				maxWidth="md"
				sx={{ minHeight: "100vh", display: "flex", flexDirection: "column" }}
			>
				<Box
					sx={{
						display: "flex",
						justifyContent: "space-between",
						alignItems: "center",
						mb: 4,
						pt: 2,
					}}
				>
					<Typography
						variant="h4"
						component="h1"
						sx={{ color: "text.primary" }}
					>
						Renta Fija App
					</Typography>
					<Button
						variant="contained"
						onClick={toggleColorMode}
						sx={{
							borderRadius: "9999px", // Botón de píldora
							background:
								mode === "light"
									? lightTheme.palette.background.paper
									: lightTheme.palette.background.default,
							"&:hover": {
								background:
									mode === "light"
										? lightTheme.palette.background.paper
										: lightTheme.palette.background.default,
							},
							color: "white",
							boxShadow: "0 4px 6px rgba(0,0,0,0.1)",
							transition: "all 0.3s ease-in-out",
						}}
					>
						{mode === "light" ? "Activar Modo Oscuro" : "Activar Modo Claro"}
					</Button>
				</Box>
				<AllAssets />
			</Container>
		</ThemeProvider>
	);
}

export default App;
