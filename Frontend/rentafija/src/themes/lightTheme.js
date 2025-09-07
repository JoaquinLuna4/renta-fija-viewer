import { createTheme } from "@mui/material/styles";
import { blueGrey } from "@mui/material/colors";

const knicksBlue = "#006BB6"; // Azul principal de los Knicks
const knicksOrange = "#F58426"; // Naranja principal de los Knicks
// const knicksSilver = "#BEC0C2"; // Gris para detalles
// const knicksBlack = "#000000"; // Negro para texto

const lightTheme = createTheme({
	palette: {
		mode: "light", // Define explícitamente el modo claro
		primary: {
			main: knicksOrange, // El naranja será el color primario
			light: "#FF994C", // Un naranja más claro
			dark: "#D16F00", // Un naranja más oscuro
			contrastText: "#efefef", // Texto en blanco sobre el naranja
		},
		secondary: {
			main: knicksBlue, // El azul será el color secundario
			light: "#3388D1", // Un azul más claro para estados hover/active
			dark: "#004C8A", // Un azul más oscuro
			contrastText: (theme) => theme.palette.primary.contrastText, // Usa el mismo contraste que el primario
		},
		error: {
			main: "#D32F2F", // Color de error estándar de Material-UI
		},
		warning: {
			main: "#FBC02D", // Color de advertencia estándar
		},
		info: {
			main: "#2196F3", // Color de información estándar
		},
		success: {
			main: "#4CAF50", // Color de éxito estándar
		},
		text: {
			primary: "rgba(22, 22, 22, 1)",
			secondary: "rgb(166, 166, 166)",
		},
		background: {
			default: "rgb(46, 46, 46)",
			paper: "#FFFFFF",
		},
		divider: blueGrey[200],
	},
	typography: {
		fontFamily: ["Roboto", "Arial", "sans-serif"].join(","),
		h1: {
			fontSize: "3.5rem",
			fontWeight: 700,
			color: knicksBlue,
		},
		h2: {
			fontSize: "2.5rem",
			fontWeight: 600,
			color: knicksBlue,
		},
		button: {
			textTransform: "none",
			fontWeight: 600,
		},
	},
	spacing: 8,
	components: {
		MuiButton: {
			styleOverrides: {
				root: {
					borderRadius: 4,
				},
				containedPrimary: {
					"&:hover": {
						backgroundColor: knicksBlue,
						opacity: 0.9,
					},
				},
				containedSecondary: {
					"&:hover": {
						backgroundColor: knicksOrange,
						opacity: 0.9,
					},
				},
			},
		},
		MuiAppBar: {
			styleOverrides: {
				colorPrimary: {
					backgroundColor: knicksBlue,
				},
			},
		},
		MuiLink: {
			styleOverrides: {
				root: {
					color: knicksBlue,
					"&:hover": {
						color: knicksOrange,
					},
				},
			},
		},
		MuiPaper: {
			styleOverrides: {
				root: {
					boxShadow: "0px 2px 10px rgba(0, 0, 0, 0.05)",
				},
			},
		},
		MuiTextField: {
			styleOverrides: {
				root: {
					"& label.Mui-focused": {
						color: knicksBlue,
					},
					"& .MuiOutlinedInput-root": {
						"&.Mui-focused fieldset": {
							borderColor: knicksBlue,
						},
					},
				},
			},
		},
	},
});

export default lightTheme;
