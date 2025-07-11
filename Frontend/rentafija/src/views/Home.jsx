import { Container } from "@mui/material";
import React from "react";
import { Typography } from "@mui/material";
import { Button } from "@mui/material";
import { Box } from "@mui/material";
import { useNavigate } from "react-router-dom";
function Home() {
	const navigate = useNavigate();

	return (
		<Container
			sx={{
				display: "flex",
				justifyContent: "center",
				alignItems: "flex-start",
				minHeight: "100vh",
				flexDirection: "column",
			}}
		>
			<Typography variant="h1" fontSize={60}>
				Renta Fija
			</Typography>
			<Typography variant="body1" fontSize={20}>
				Inviertí informado con los datos que necesitas para tomar decisiones
				acertadas.
			</Typography>
			<Box display="flex" justifyContent="flex-end" width="100%">
				<Button
					variant="contained"
					color="primary"
					sx={{
						marginTop: 4,
						fontSize: 16,
					}}
					onClick={() => navigate("/allAssets")}
				>
					Ver Informe
				</Button>
			</Box>
		</Container>
	);
}

export default Home;
