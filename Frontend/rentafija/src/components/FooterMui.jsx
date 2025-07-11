import React from "react";
import { Box, Typography, Link } from "@mui/material";

function FooterMui() {
	return (
		<Box
			component="footer"
			sx={{
				py: 0.5,
				px: 2,
				mt: "auto",
				backgroundColor: (theme) =>
					theme.palette.mode === "light"
						? theme.palette.grey[200]
						: theme.palette.grey[800],
				textAlign: "center",
			}}
		>
			<Typography variant="body2" color="text.secondary">
				&copy; {new Date().getFullYear()} Joaquín Luna. Todos los derechos
				reservados.
			</Typography>
			<Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
				Contacto:{" "}
				<Link color="inherit" href="mailto:joaquinfluna4@gmail.com">
					joaquinfluna4@gmail.com
				</Link>
			</Typography>
		</Box>
	);
}

export default FooterMui;
